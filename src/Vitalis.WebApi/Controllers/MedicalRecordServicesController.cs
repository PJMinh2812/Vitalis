using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vitalis.Application.DTOs.MedicalRecords;
using Vitalis.Application.Interfaces;

namespace Vitalis.WebApi.Controllers;

// VC-14 #2/#3 — no dedicated "lab technician" role exists among the 5 seeded
// roles, so these stay Doctor-only, same as the rest of the exam screen.
[ApiController]
[Route("api/medical-record-services")]
[Authorize(Roles = "Doctor")]
public class MedicalRecordServicesController(IMedicalRecordsService medicalRecordsService) : ControllerBase
{
    [HttpPatch("{id:int}/cancel")]
    public async Task<IActionResult> Cancel(int id, CancellationToken cancellationToken)
    {
        await medicalRecordsService.CancelServiceAsync(id, cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:int}/result")]
    public async Task<ActionResult<MedicalRecordServiceDto>> EnterResult(int id, LabResultRequest request, CancellationToken cancellationToken)
    {
        var performedBy = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await medicalRecordsService.EnterResultAsync(id, request, performedBy, cancellationToken);
        return Ok(result);
    }
}
