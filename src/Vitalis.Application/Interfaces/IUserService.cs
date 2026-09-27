using Vitalis.Application.Common;
using Vitalis.Application.DTOs.Users;

namespace Vitalis.Application.Interfaces;

public interface IUserService
{
    // Reusable by any flow that needs a login-capable account (self-registration
    // now; Doctor/Admin staff creation later) — hashes the password and assigns
    // the given roles (which must already exist) in one call.
    Task<int> CreateUserAsync(
        string username,
        string rawPassword,
        string? fullName,
        string? email,
        string? phone,
        IReadOnlyList<string> roleNames,
        CancellationToken cancellationToken = default);

    Task<PagedResult<UserDto>> SearchAsync(string? keyword, string? role, int page, int pageSize, CancellationToken cancellationToken = default);

    Task<UserDto> AssignRolesAsync(int userId, IReadOnlyList<int> roleIds, int assignedBy, CancellationToken cancellationToken = default);

    // Returns the new temporary password once — the caller (Controller) must
    // hand it back to the admin immediately; it is never stored or logged raw.
    Task<ResetPasswordResult> ResetPasswordAsync(int userId, CancellationToken cancellationToken = default);

    Task<UserDto> SetLockAsync(int userId, LockUserRequest request, CancellationToken cancellationToken = default);
}
