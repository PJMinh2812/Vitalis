using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vitalis.Application.DTOs.Billing;
using Vitalis.Application.Interfaces;

namespace Vitalis.WebApi.Controllers;

// Minimal service catalog — groundwork for VC-14 (order during a visit); the
// full admin screen (VC-21) is step 9's scope.
[ApiController]
public class ServicesController(IServiceCatalogService serviceCatalogService) : ControllerBase
{
    [HttpGet("api/services")]
    public async Task<ActionResult<IReadOnlyList<ServiceDto>>> GetAll([FromQuery] bool? active, CancellationToken cancellationToken)
    {
        var result = await serviceCatalogService.GetAllAsync(active, cancellationToken);
        return Ok(result);
    }

    [HttpPost("api/admin/services")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ServiceDto>> Create(CreateServiceRequest request, CancellationToken cancellationToken)
    {
        var result = await serviceCatalogService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetAll), result);
    }

    [HttpPatch("api/admin/services/{id:int}/deactivate")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Deactivate(int id, CancellationToken cancellationToken)
    {
        await serviceCatalogService.DeactivateAsync(id, cancellationToken);
        return NoContent();
    }
}
