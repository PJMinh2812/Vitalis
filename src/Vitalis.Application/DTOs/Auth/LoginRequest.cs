namespace Vitalis.Application.DTOs.Auth;

// "Username" accepts either the username or the email (VC-01 UI label:
// "Tên đăng nhập / Email").
public record LoginRequest
{
    public required string Username { get; init; }
    public required string Password { get; init; }
}
