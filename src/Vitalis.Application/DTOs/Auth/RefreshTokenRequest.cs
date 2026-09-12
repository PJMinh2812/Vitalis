namespace Vitalis.Application.DTOs.Auth;

public record RefreshTokenRequest
{
    public required string RefreshToken { get; init; }
}
