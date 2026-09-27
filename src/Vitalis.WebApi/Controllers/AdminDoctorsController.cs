using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vitalis.Application.DTOs.Doctors;
using Vitalis.Application.Interfaces;

namespace Vitalis.WebApi.Controllers;

// VC-19 — every action here is Admin-only.
[ApiController]
[Authorize(Roles = "Admin")]
public class AdminDoctorsController(IDoctorService doctorService) : ControllerBase
{
    // POST /api/admin/doctors — creates the login account (auth.users + Doctor role) and the doctor profile.
    [HttpPost("api/admin/doctors")]
    public async Task<ActionResult<DoctorDto>> Create(DoctorRequest request, CancellationToken cancellationToken)
    {
        var result = await doctorService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(DoctorsController.GetById), "Doctors", new { id = result.Id }, result);
    }

    [HttpPut("api/admin/doctors/{id:int}")]
    public async Task<ActionResult<DoctorDto>> Update(int id, UpdateDoctorRequest request, CancellationToken cancellationToken)
    {
        var result = await doctorService.UpdateAsync(id, request, cancellationToken);
        return Ok(result);
    }

    // PATCH /api/admin/doctors/{id}/deactivate — blocked if the doctor still has non-completed appointments.
    [HttpPatch("api/admin/doctors/{id:int}/deactivate")]
    public async Task<IActionResult> Deactivate(int id, CancellationToken cancellationToken)
    {
        await doctorService.DeactivateAsync(id, cancellationToken);
        return NoContent();
    }
}
