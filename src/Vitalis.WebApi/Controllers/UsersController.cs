using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vitalis.Application.Common;
using Vitalis.Application.DTOs.Users;
using Vitalis.Application.Interfaces;

namespace Vitalis.WebApi.Controllers;

// VC-22 — every action here is Admin-only.
[ApiController]
[Route("api/admin/users")]
[Authorize(Roles = "Admin")]
public class UsersController(IUserService userService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResult<UserDto>>> Search(
        [FromQuery] string? keyword,
        [FromQuery] string? role,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await userService.SearchAsync(keyword, role, page, pageSize, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:int}/roles")]
    public async Task<ActionResult<UserDto>> AssignRoles(int id, AssignRolesRequest request, CancellationToken cancellationToken)
    {
        var assignedBy = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await userService.AssignRolesAsync(id, request.RoleIds, assignedBy, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:int}/reset-password")]
    public async Task<ActionResult<ResetPasswordResult>> ResetPassword(int id, CancellationToken cancellationToken)
    {
        var result = await userService.ResetPasswordAsync(id, cancellationToken);
        return Ok(result);
    }

    [HttpPatch("{id:int}/lock")]
    public async Task<ActionResult<UserDto>> SetLock(int id, LockUserRequest request, CancellationToken cancellationToken)
    {
        var result = await userService.SetLockAsync(id, request, cancellationToken);
        return Ok(result);
    }
}
