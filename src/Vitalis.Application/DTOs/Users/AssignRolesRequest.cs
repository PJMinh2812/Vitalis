namespace Vitalis.Application.DTOs.Users;

public record AssignRolesRequest
{
    public required IReadOnlyList<int> RoleIds { get; init; }
}
