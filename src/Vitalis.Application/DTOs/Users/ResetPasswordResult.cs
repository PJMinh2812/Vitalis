namespace Vitalis.Application.DTOs.Users;

// Shown to the admin exactly once — never stored or logged in plain text.
public record ResetPasswordResult(string TemporaryPassword);
