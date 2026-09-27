using Vitalis.Application.DTOs.Auth;

namespace Vitalis.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResponse> LoginAsync(LoginRequest request, string? ipAddress, string? deviceInfo, CancellationToken cancellationToken = default);

    Task<AuthResponse> RegisterAsync(RegisterPatientRequest request, string? ipAddress, string? deviceInfo, CancellationToken cancellationToken = default);

    Task<AuthResponse> RefreshAsync(string refreshToken, string? ipAddress, string? deviceInfo, CancellationToken cancellationToken = default);

    Task LogoutAsync(string refreshToken, CancellationToken cancellationToken = default);
}
