using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vitalis.Application.DTOs.Reports;
using Vitalis.Application.Interfaces;

namespace Vitalis.WebApi.Controllers;

// VC-23 — Excel export (#2) is out of scope, needs a new spreadsheet package.
[ApiController]
[Authorize(Roles = "Admin")]
public class ReportsController(IReportService reportService) : ControllerBase
{
    [HttpGet("api/admin/reports")]
    public async Task<ActionResult<ClinicReportDto>> GetReport(
        [FromQuery] DateOnly from, [FromQuery] DateOnly to, [FromQuery] int? doctorId, CancellationToken cancellationToken)
    {
        var result = await reportService.GetClinicReportAsync(from, to, doctorId, cancellationToken);
        return Ok(result);
    }
}
