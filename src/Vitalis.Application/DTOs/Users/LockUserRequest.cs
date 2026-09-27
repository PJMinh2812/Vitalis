namespace Vitalis.Application.DTOs.Users;

public record LockUserRequest
{
    public required bool IsActive { get; init; }
    public string? Reason { get; init; }
}
