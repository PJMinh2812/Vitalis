using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vitalis.Application.DTOs.Pharmacy;
using Vitalis.Application.Interfaces;

namespace Vitalis.WebApi.Controllers;

[ApiController]
[Route("api/prescriptions")]
[Authorize(Roles = "Pharmacist,Admin")]
public class PrescriptionsController(IPharmacyService pharmacyService) : ControllerBase
{
    // POST /api/prescriptions/{id}/dispense   (VC-16 #1)
    [HttpPost("{id:int}/dispense")]
    public async Task<ActionResult<DispenseResultDto>> Dispense(int id, CancellationToken cancellationToken)
    {
        var dispensedBy = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await pharmacyService.DispenseAsync(id, dispensedBy, cancellationToken);
        return Ok(result);
    }
}
