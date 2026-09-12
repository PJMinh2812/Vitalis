using Vitalis.Application.Common;
using Vitalis.Application.DTOs.Auth;
using Vitalis.Application.Exceptions;
using Vitalis.Application.Interfaces;
using Vitalis.Domain.Entities.Auth;
using Vitalis.Domain.Entities.Scheduling;

namespace Vitalis.Application.Services;

public class AuthService(
    IUnitOfWork unitOfWork,
    IUserService userService,
    IPatientService patientService,
    IPasswordHasher passwordHasher,
    IJwtTokenGenerator tokenGenerator,
    JwtSettings jwtSettings) : IAuthService
{
    private const int MaxFailedLoginAttempts = 5;
    private static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(30);

    public async Task<AuthResponse> LoginAsync(LoginRequest request, string? ipAddress, string? deviceInfo, CancellationToken cancellationToken = default)
    {
        var userRepository = unitOfWork.Repository<User>();
        // VC-01 label is "Tên đăng nhập / Email" — either identifies the account.
        var user = await userRepository.FirstOrDefaultAsync(
            userRepository.Query().Where(u => !u.IsDeleted && (u.Username == request.Username || u.Email == request.Username)),
            cancellationToken);

        if (user is null)
            throw new UnauthorizedException("Tên đăng nhập hoặc mật khẩu không đúng");

        if (!user.IsActive)
            throw new ForbiddenException("Tài khoản đã bị khoá, vui lòng liên hệ quản trị viên");

        if (user.LockoutEnd is not null && user.LockoutEnd > DateTime.UtcNow)
            throw new ForbiddenException($"Tài khoản tạm khoá do nhập sai quá {MaxFailedLoginAttempts} lần. Vui lòng thử lại sau {user.LockoutEnd:HH:mm dd/MM}");

        if (!passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            user.FailedLoginCount++;
            if (user.FailedLoginCount >= MaxFailedLoginAttempts)
            {
                user.LockoutEnd = DateTime.UtcNow.Add(LockoutDuration);
                user.FailedLoginCount = 0;
            }
            userRepository.Update(user);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            throw new UnauthorizedException("Tên đăng nhập hoặc mật khẩu không đúng");
        }

        user.FailedLoginCount = 0;
        user.LockoutEnd = null;
        user.LastLoginAt = DateTime.UtcNow;
        userRepository.Update(user);

        return await IssueTokensAsync(user, ipAddress, deviceInfo, cancellationToken);
    }

    public async Task<AuthResponse> RegisterAsync(RegisterPatientRequest request, string? ipAddress, string? deviceInfo, CancellationToken cancellationToken = default)
    {
        var userId = await userService.CreateUserAsync(
            request.Phone, request.Password, request.FullName, request.Email, request.Phone, ["Patient"], cancellationToken);

        // Links an existing walk-in profile with this phone (keeps visit history)
        // or creates a fresh one — see PatientService.LinkOrCreateAsync.
        await patientService.LinkOrCreateAsync(userId, request.FullName, request.Phone, request.DateOfBirth, cancellationToken);

        var userRepository = unitOfWork.Repository<User>();
        var user = await userRepository.GetByIdAsync(userId, cancellationToken)
            ?? throw new NotFoundException(nameof(User), userId);

        return await IssueTokensAsync(user, ipAddress, deviceInfo, cancellationToken);
    }

    public async Task<AuthResponse> RefreshAsync(string refreshToken, string? ipAddress, string? deviceInfo, CancellationToken cancellationToken = default)
    {
        var tokenHash = TokenHasher.Sha256(refreshToken);
        var refreshTokenRepository = unitOfWork.Repository<RefreshToken>();
        var existing = await refreshTokenRepository.FirstOrDefaultAsync(
            refreshTokenRepository.Query().Where(t => t.TokenHash == tokenHash), cancellationToken);

        if (existing is null || existing.RevokedAt is not null || existing.ExpiresAt <= DateTime.UtcNow)
            throw new UnauthorizedException("Refresh token không hợp lệ hoặc đã hết hạn");

        var userRepository = unitOfWork.Repository<User>();
        var user = await userRepository.GetByIdAsync(existing.UserId, cancellationToken)
            ?? throw new UnauthorizedException("Refresh token không hợp lệ hoặc đã hết hạn");

        if (!user.IsActive)
            throw new ForbiddenException("Tài khoản đã bị khoá, vui lòng liên hệ quản trị viên");

        var response = await IssueTokensAsync(user, ipAddress, deviceInfo, cancellationToken);

        // Rotation: retire the presented token and point it at its replacement,
        // so a stolen-but-already-used refresh token is a dead end, not reusable.
        existing.RevokedAt = DateTime.UtcNow;
        existing.ReplacedByTokenHash = TokenHasher.Sha256(response.RefreshToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return response;
    }

    public async Task LogoutAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        var tokenHash = TokenHasher.Sha256(refreshToken);
        var refreshTokenRepository = unitOfWork.Repository<RefreshToken>();
        var existing = await refreshTokenRepository.FirstOrDefaultAsync(
            refreshTokenRepository.Query().Where(t => t.TokenHash == tokenHash && t.RevokedAt == null), cancellationToken);

        if (existing is null)
            return; // already logged out or unknown token — logout is idempotent

        existing.RevokedAt = DateTime.UtcNow;
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task<AuthResponse> IssueTokensAsync(User user, string? ipAddress, string? deviceInfo, CancellationToken cancellationToken)
    {
        var userRoleRepository = unitOfWork.Repository<UserRole>();
        var userRoles = await userRoleRepository.ToListAsync(
            userRoleRepository.Query().Where(ur => ur.UserId == user.Id), cancellationToken);
        var roleIds = userRoles.Select(ur => ur.RoleId).ToHashSet();

        var roleRepository = unitOfWork.Repository<Role>();
        var roleNames = (await roleRepository.ToListAsync(roleRepository.Query().Where(r => roleIds.Contains(r.Id)), cancellationToken))
            .Select(r => r.Name).ToList();

        var doctorRepository = unitOfWork.Repository<Doctor>();
        var doctor = await doctorRepository.FirstOrDefaultAsync(doctorRepository.Query().Where(d => d.UserId == user.Id), cancellationToken);

        var patientRepository = unitOfWork.Repository<Patient>();
        var patient = await patientRepository.FirstOrDefaultAsync(patientRepository.Query().Where(p => p.UserId == user.Id), cancellationToken);

        var accessToken = tokenGenerator.GenerateAccessToken(user.Id, user.Username, roleNames, doctor?.Id, patient?.Id);
        var refreshToken = tokenGenerator.GenerateRefreshToken();

        var refreshTokenRepository = unitOfWork.Repository<RefreshToken>();
        await refreshTokenRepository.AddAsync(new RefreshToken
        {
            UserId = user.Id,
            TokenHash = TokenHasher.Sha256(refreshToken),
            DeviceInfo = deviceInfo,
            IpAddress = ipAddress,
            ExpiresAt = DateTime.UtcNow.AddDays(jwtSettings.RefreshTokenDays),
        }, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new AuthResponse(
            accessToken,
            refreshToken,
            jwtSettings.AccessTokenMinutes * 60,
            new UserSummaryDto(user.Id, user.FullName ?? user.Username, roleNames, doctor?.Id, patient?.Id));
    }
}
