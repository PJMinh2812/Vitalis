using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vitalis.Application.DTOs.Doctors;
using Vitalis.Application.Interfaces;

namespace Vitalis.WebApi.Controllers;

// VC-20 — every action here is Admin-only.
[ApiController]
[Authorize(Roles = "Admin")]
public class TimeOffController(IDoctorTimeOffService timeOffService) : ControllerBase
{
    // GET /api/admin/time-off?doctorId=   (omit doctorId to list clinic-wide entries too)
    [HttpGet("api/admin/time-off")]
    public async Task<ActionResult<IReadOnlyList<DoctorTimeOffDto>>> GetByDoctor([FromQuery] int? doctorId, CancellationToken cancellationToken)
    {
        var result = await timeOffService.GetByDoctorAsync(doctorId, cancellationToken);
        return Ok(result);
    }

    // POST /api/admin/time-off — never blocks on conflicts, returns them so reception can reschedule.
    [HttpPost("api/admin/time-off")]
    public async Task<ActionResult<CreateTimeOffResult>> Create(CreateTimeOffRequest request, CancellationToken cancellationToken)
    {
        var result = await timeOffService.CreateAsync(request, cancellationToken);
        return Ok(result);
    }
}
