using Microsoft.AspNetCore.Mvc;
using Vitalis.Application.Common;
using Vitalis.Application.DTOs.Patients;
using Vitalis.Application.Interfaces;

namespace Vitalis.WebApi.Controllers;

[ApiController]
[Route("api/patients")]
public class PatientsController(IPatientService patientService) : ControllerBase
{
    // GET /api/patients?keyword=&page=&pageSize=   (VC-08, VC-09)
    [HttpGet]
    public async Task<ActionResult<PagedResult<PatientDto>>> Search(
        [FromQuery] string? keyword,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await patientService.SearchAsync(keyword, page, pageSize, cancellationToken);
        return Ok(result);
    }

    // GET /api/patients/lookup?phone=   (VC-02, called before login)
    [HttpGet("lookup")]
    public async Task<ActionResult<PatientLookupResult>> LookupByPhone(
        [FromQuery] string phone,
        CancellationToken cancellationToken = default)
    {
        var result = await patientService.LookupByPhoneAsync(phone, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<PatientDto>> GetById(int id, CancellationToken cancellationToken = default)
    {
        var result = await patientService.GetByIdAsync(id, cancellationToken);
        return Ok(result);
    }

    // POST /api/patients   (VC-08 endpoint 2, full profile)
    [HttpPost]
    public async Task<ActionResult<PatientDto>> Create(
        CreatePatientRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await patientService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    // POST /api/patients/quick   (VC-09, walk-in/phone booking)
    [HttpPost("quick")]
    public async Task<ActionResult<PatientDto>> QuickCreate(
        QuickCreatePatientRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await patientService.QuickCreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    // PUT /api/patients/{id}   (VC-08 endpoint 2, edit)
    [HttpPut("{id:int}")]
    public async Task<ActionResult<PatientDto>> Update(
        int id,
        UpdatePatientRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await patientService.UpdateAsync(id, request, cancellationToken);
        return Ok(result);
    }
}
