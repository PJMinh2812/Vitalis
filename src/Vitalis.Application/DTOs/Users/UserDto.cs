namespace Vitalis.Application.DTOs.Users;

public record UserDto(
    int Id,
    string Username,
    string? FullName,
    string? Email,
    IReadOnlyList<string> Roles,
    DateTime? LastLoginAt,
    bool IsActive);
