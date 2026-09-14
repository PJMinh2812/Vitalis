using Microsoft.AspNetCore.Mvc;
using Vitalis.Application.Common;
using Vitalis.Application.DTOs.Doctors;
using Vitalis.Application.Interfaces;

namespace Vitalis.WebApi.Controllers;

// Public/patient-facing doctor endpoints (VC-03 booking). Admin write actions
// live in AdminDoctorsController under /api/admin/doctors.
[ApiController]
public class DoctorsController(IDoctorService doctorService) : ControllerBase
{
    // GET /api/doctors?specialtyId=&keyword=&page=&pageSize=
    [HttpGet("api/doctors")]
    public async Task<ActionResult<PagedResult<DoctorDto>>> Search(
        [FromQuery] string? keyword,
        [FromQuery] int? specialtyId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await doctorService.SearchAsync(keyword, specialtyId, page, pageSize, cancellationToken);
        return Ok(result);
    }

    [HttpGet("api/doctors/{id:int}")]
    public async Task<ActionResult<DoctorDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await doctorService.GetByIdAsync(id, cancellationToken);
        return Ok(result);
    }

    // GET /api/doctors/{id}/available-slots?date=2026-10-01
    [HttpGet("api/doctors/{id:int}/available-slots")]
    public async Task<ActionResult<IReadOnlyList<SlotDto>>> GetAvailableSlots(int id, [FromQuery] DateOnly date, CancellationToken cancellationToken)
    {
        var result = await doctorService.GetAvailableSlotsAsync(id, date, cancellationToken);
        return Ok(result);
    }
}
