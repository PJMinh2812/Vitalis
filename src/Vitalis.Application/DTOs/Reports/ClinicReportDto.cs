namespace Vitalis.Application.DTOs.Reports;

// ExamRevenue/ServiceRevenue are billed amounts (invoice_items), not
// payment-prorated cash — see learning-notes/13 for why. TotalRevenue at the
// top level, however, is real cash collected (sum of Payments in range), per
// VC-23's explicit "Doanh thu ghi nhận theo payments (tiền thực thu)".
public record DoctorReportRow(
    int DoctorId,
    string DoctorName,
    int VisitCount,
    decimal ExamRevenue,
    decimal ServiceRevenue,
    int NoShowCount);

public record ClinicReportDto(
    DateOnly From,
    DateOnly To,
    int TotalVisits,
    decimal TotalRevenue,
    decimal OutstandingAmount,
    double NoShowRate,
    List<DoctorReportRow> ByDoctor);
