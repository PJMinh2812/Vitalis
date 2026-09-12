namespace Vitalis.Application.DTOs.Auth;

public record ForgotPasswordRequest
{
    public required string Email { get; init; }
}
