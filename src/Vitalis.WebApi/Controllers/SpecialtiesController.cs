using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vitalis.Application.DTOs.Doctors;
using Vitalis.Application.Interfaces;

namespace Vitalis.WebApi.Controllers;

[ApiController]
public class SpecialtiesController(ISpecialtyService specialtyService) : ControllerBase
{
    // GET /api/specialties?active=true   (VC-03 dropdown, VC-19 admin table)
    [HttpGet("api/specialties")]
    public async Task<ActionResult<IReadOnlyList<SpecialtyDto>>> GetAll([FromQuery] bool? active, CancellationToken cancellationToken)
    {
        var result = await specialtyService.GetAllAsync(active, cancellationToken);
        return Ok(result);
    }

    // POST /api/admin/specialties   (VC-19)
    [HttpPost("api/admin/specialties")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<SpecialtyDto>> Create(CreateSpecialtyRequest request, CancellationToken cancellationToken)
    {
        var result = await specialtyService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetAll), result);
    }
}
