using System.Security.Cryptography;
using Vitalis.Application.Common;
using Vitalis.Application.DTOs.Users;
using Vitalis.Application.Exceptions;
using Vitalis.Application.Interfaces;
using Vitalis.Domain.Entities.Auth;

namespace Vitalis.Application.Services;

public class UserService(IUnitOfWork unitOfWork, IPasswordHasher passwordHasher) : IUserService
{
    public async Task<int> CreateUserAsync(
        string username,
        string rawPassword,
        string? fullName,
        string? email,
        string? phone,
        IReadOnlyList<string> roleNames,
        CancellationToken cancellationToken = default)
    {
        var userRepository = unitOfWork.Repository<User>();
        var usernameTaken = await userRepository.FirstOrDefaultAsync(
            userRepository.Query().Where(u => u.Username == username), cancellationToken) is not null;
        if (usernameTaken)
            throw new ConflictException($"Tên đăng nhập '{username}' đã được sử dụng");

        var user = new User
        {
            Username = username,
            PasswordHash = passwordHasher.Hash(rawPassword),
            FullName = fullName,
            Email = email,
            Phone = phone,
        };
        await userRepository.AddAsync(user, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var roleRepository = unitOfWork.Repository<Role>();
        var roles = await roleRepository.ToListAsync(
            roleRepository.Query().Where(r => roleNames.Contains(r.Name)), cancellationToken);

        var userRoleRepository = unitOfWork.Repository<UserRole>();
        foreach (var role in roles)
            await userRoleRepository.AddAsync(new UserRole { UserId = user.Id, RoleId = role.Id }, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return user.Id;
    }

    public async Task<PagedResult<UserDto>> SearchAsync(string? keyword, string? role, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var userRepository = unitOfWork.Repository<User>();
        var userRoleRepository = unitOfWork.Repository<UserRole>();
        var roleRepository = unitOfWork.Repository<Role>();

        var query = userRepository.Query().Where(u => !u.IsDeleted);

        if (!string.IsNullOrWhiteSpace(keyword))
            query = query.Where(u => u.Username.StartsWith(keyword) || (u.FullName != null && u.FullName.StartsWith(keyword)));

        if (!string.IsNullOrWhiteSpace(role))
        {
            var matchingRole = await roleRepository.FirstOrDefaultAsync(roleRepository.Query().Where(r => r.Name == role), cancellationToken);
            HashSet<int> userIdsWithRole = matchingRole is null
                ? []
                : (await userRoleRepository.ToListAsync(userRoleRepository.Query().Where(ur => ur.RoleId == matchingRole.Id), cancellationToken))
                    .Select(ur => ur.UserId).ToHashSet();

            query = query.Where(u => userIdsWithRole.Contains(u.Id));
        }

        query = query.OrderBy(u => u.Username);

        var paged = await userRepository.GetPagedAsync(query, page, pageSize, cancellationToken);

        // Batch-load roles for the whole page and join in memory instead of N+1
        // querying per user (Application deliberately has no EF Core Include here).
        var pageUserIds = paged.Items.Select(u => u.Id).ToList();
        var pageUserRoles = await userRoleRepository.ToListAsync(
            userRoleRepository.Query().Where(ur => pageUserIds.Contains(ur.UserId)), cancellationToken);
        var allRoles = await roleRepository.ToListAsync(roleRepository.Query(), cancellationToken);
        var roleNameById = allRoles.ToDictionary(r => r.Id, r => r.Name);

        var rolesByUserId = pageUserRoles
            .GroupBy(ur => ur.UserId)
            .ToDictionary(g => g.Key, g => (IReadOnlyList<string>)g.Select(ur => roleNameById[ur.RoleId]).ToList());

        var items = paged.Items.Select(u => ToDto(u, rolesByUserId.GetValueOrDefault(u.Id, []))).ToList();

        return new PagedResult<UserDto>(items, paged.TotalCount, paged.Page, paged.PageSize);
    }

    public async Task<UserDto> AssignRolesAsync(int userId, IReadOnlyList<int> roleIds, int assignedBy, CancellationToken cancellationToken = default)
    {
        var userRepository = unitOfWork.Repository<User>();
        var user = await userRepository.GetByIdAsync(userId, cancellationToken)
            ?? throw new NotFoundException(nameof(User), userId);

        var userRoleRepository = unitOfWork.Repository<UserRole>();
        var existing = await userRoleRepository.ToListAsync(
            userRoleRepository.Query().Where(ur => ur.UserId == userId), cancellationToken);

        foreach (var stale in existing.Where(ur => !roleIds.Contains(ur.RoleId)))
            userRoleRepository.Remove(stale);

        var existingRoleIds = existing.Select(ur => ur.RoleId).ToHashSet();
        foreach (var roleId in roleIds.Where(id => !existingRoleIds.Contains(id)))
            await userRoleRepository.AddAsync(new UserRole { UserId = userId, RoleId = roleId, AssignedBy = assignedBy }, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return await BuildUserDtoAsync(user, cancellationToken);
    }

    public async Task<ResetPasswordResult> ResetPasswordAsync(int userId, CancellationToken cancellationToken = default)
    {
        var userRepository = unitOfWork.Repository<User>();
        var user = await userRepository.GetByIdAsync(userId, cancellationToken)
            ?? throw new NotFoundException(nameof(User), userId);

        var temporaryPassword = Convert.ToBase64String(RandomNumberGenerator.GetBytes(9));
        user.PasswordHash = passwordHasher.Hash(temporaryPassword);
        user.UpdatedAt = DateTime.UtcNow;
        userRepository.Update(user);

        await RevokeAllRefreshTokensAsync(userId, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new ResetPasswordResult(temporaryPassword);
    }

    public async Task<UserDto> SetLockAsync(int userId, LockUserRequest request, CancellationToken cancellationToken = default)
    {
        var userRepository = unitOfWork.Repository<User>();
        var user = await userRepository.GetByIdAsync(userId, cancellationToken)
            ?? throw new NotFoundException(nameof(User), userId);

        user.IsActive = request.IsActive;
        user.UpdatedAt = DateTime.UtcNow;
        userRepository.Update(user);

        if (!request.IsActive)
            await RevokeAllRefreshTokensAsync(userId, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return await BuildUserDtoAsync(user, cancellationToken);
    }

    private async Task RevokeAllRefreshTokensAsync(int userId, CancellationToken cancellationToken)
    {
        var refreshTokenRepository = unitOfWork.Repository<RefreshToken>();
        var activeTokens = await refreshTokenRepository.ToListAsync(
            refreshTokenRepository.Query().Where(t => t.UserId == userId && t.RevokedAt == null), cancellationToken);

        // Still tracked from the query above, on the same DbContext — no explicit
        // Update() needed, SaveChangesAsync will pick up the change.
        foreach (var token in activeTokens)
            token.RevokedAt = DateTime.UtcNow;
    }

    private async Task<UserDto> BuildUserDtoAsync(User user, CancellationToken cancellationToken)
    {
        var userRoleRepository = unitOfWork.Repository<UserRole>();
        var userRoles = await userRoleRepository.ToListAsync(
            userRoleRepository.Query().Where(ur => ur.UserId == user.Id), cancellationToken);
        var roleIds = userRoles.Select(ur => ur.RoleId).ToList();

        var roleRepository = unitOfWork.Repository<Role>();
        var roles = await roleRepository.ToListAsync(roleRepository.Query().Where(r => roleIds.Contains(r.Id)), cancellationToken);

        return ToDto(user, roles.Select(r => r.Name).ToList());
    }

    private static UserDto ToDto(User user, IReadOnlyList<string> roles) =>
        new(user.Id, user.Username, user.FullName, user.Email, roles, user.LastLoginAt, user.IsActive);
}
