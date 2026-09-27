using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vitalis.Application.DTOs.Doctors;
using Vitalis.Application.Interfaces;

namespace Vitalis.WebApi.Controllers;

// VC-20 — every action here is Admin-only.
[ApiController]
[Authorize(Roles = "Admin")]
public class DoctorSchedulesController(IDoctorScheduleService scheduleService) : ControllerBase
{
    // GET /api/admin/doctor-schedules?doctorId=
    [HttpGet("api/admin/doctor-schedules")]
    public async Task<ActionResult<IReadOnlyList<DoctorScheduleDto>>> GetByDoctor([FromQuery] int doctorId, CancellationToken cancellationToken)
    {
        var result = await scheduleService.GetByDoctorAsync(doctorId, cancellationToken);
        return Ok(result);
    }

    [HttpPost("api/admin/doctor-schedules")]
    public async Task<ActionResult<DoctorScheduleDto>> Create(ScheduleRequest request, CancellationToken cancellationToken)
    {
        var result = await scheduleService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetByDoctor), new { doctorId = result.DoctorId }, result);
    }
}
