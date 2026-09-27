using Microsoft.AspNetCore.Mvc;
using Vitalis.Application.DTOs.Auth;
using Vitalis.Application.Interfaces;

namespace Vitalis.WebApi.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(IAuthService authService) : ControllerBase
{
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        var result = await authService.LoginAsync(request, GetIpAddress(), GetDeviceInfo(), cancellationToken);
        return Ok(result);
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register(RegisterPatientRequest request, CancellationToken cancellationToken)
    {
        var result = await authService.RegisterAsync(request, GetIpAddress(), GetDeviceInfo(), cancellationToken);
        return Ok(result);
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<AuthResponse>> Refresh(RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        var result = await authService.RefreshAsync(request.RefreshToken, GetIpAddress(), GetDeviceInfo(), cancellationToken);
        return Ok(result);
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout(RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        await authService.LogoutAsync(request.RefreshToken, cancellationToken);
        return NoContent();
    }

    // No SMTP configured yet — always responds the same way whether or not the
    // email exists, so the endpoint can't be used to probe who's registered.
    [HttpPost("forgot-password")]
    public IActionResult ForgotPassword(ForgotPasswordRequest request) => Accepted();

    private string? GetIpAddress() => HttpContext.Connection.RemoteIpAddress?.ToString();

    private string? GetDeviceInfo() => Request.Headers.UserAgent.ToString() is { Length: > 0 } ua ? ua : null;
}
