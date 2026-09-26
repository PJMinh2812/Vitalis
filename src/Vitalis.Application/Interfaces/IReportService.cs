using Vitalis.Application.DTOs.Reports;

namespace Vitalis.Application.Interfaces;

public interface IReportService
{
    // VC-23 — Excel export (VC-23 #2) is out of scope: it needs a new charting/
    // spreadsheet package, not just business logic (see learning-notes/13).
    Task<ClinicReportDto> GetClinicReportAsync(DateOnly from, DateOnly to, int? doctorId, CancellationToken cancellationToken = default);
}
