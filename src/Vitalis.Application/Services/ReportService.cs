using Vitalis.Application.DTOs.Reports;
using Vitalis.Application.Interfaces;
using Vitalis.Domain.Entities.Billing;
using Vitalis.Domain.Entities.Scheduling;
using Vitalis.Domain.Enums;

namespace Vitalis.Application.Services;

public class ReportService(IUnitOfWork unitOfWork) : IReportService
{
    public async Task<ClinicReportDto> GetClinicReportAsync(DateOnly from, DateOnly to, int? doctorId, CancellationToken cancellationToken = default)
    {
        var rangeStart = from.ToDateTime(TimeOnly.MinValue);
        var rangeEndExclusive = to.ToDateTime(TimeOnly.MinValue).AddDays(1);

        var appointmentRepository = unitOfWork.Repository<Appointment>();
        var appointmentQuery = appointmentRepository.Query().Where(a => a.StartTime >= rangeStart && a.StartTime < rangeEndExclusive);
        if (doctorId is not null)
            appointmentQuery = appointmentQuery.Where(a => a.DoctorId == doctorId);
        var appointments = await appointmentRepository.ToListAsync(appointmentQuery, cancellationToken);

        var totalVisits = appointments.Count(a => a.Status == AppointmentStatus.Completed);
        var noShowCount = appointments.Count(a => a.Status == AppointmentStatus.NoShow);
        var noShowRate = appointments.Count == 0 ? 0 : (double)noShowCount / appointments.Count;

        // "Doanh thu ghi nhận theo payments (tiền thực thu)" — cash collected in
        // the period, independent of which visit it belongs to (an old debt paid
        // off this month still counts as this month's revenue).
        var paymentRepository = unitOfWork.Repository<Payment>();
        var payments = await paymentRepository.ToListAsync(
            paymentRepository.Query().Where(p => p.PaidAt >= rangeStart && p.PaidAt < rangeEndExclusive), cancellationToken);
        var totalRevenue = payments.Sum(p => p.Amount); // refunds are already negative, no separate branch needed

        var invoiceRepository = unitOfWork.Repository<Invoice>();
        var appointmentIds = appointments.Select(a => a.Id).ToHashSet();
        var invoices = await invoiceRepository.ToListAsync(
            invoiceRepository.Query().Where(i => i.AppointmentId != null && appointmentIds.Contains(i.AppointmentId!.Value)), cancellationToken);

        var outstandingAmount = invoices
            .Where(i => i.CancelledAt is null)
            .Sum(i => i.TotalAmount - i.InsuranceAmount - i.PaidAmount);

        var invoiceItemRepository = unitOfWork.Repository<InvoiceItem>();
        var invoiceIds = invoices.Select(i => i.Id).ToList();
        var invoiceItems = invoiceIds.Count == 0
            ? []
            : await invoiceItemRepository.ToListAsync(invoiceItemRepository.Query().Where(i => invoiceIds.Contains(i.InvoiceId)), cancellationToken);
        var itemsByInvoiceId = invoiceItems.GroupBy(i => i.InvoiceId).ToDictionary(g => g.Key, g => g.ToList());
        var invoiceByAppointmentId = invoices.Where(i => i.AppointmentId is not null).ToDictionary(i => i.AppointmentId!.Value);

        var doctorRepository = unitOfWork.Repository<Doctor>();
        var doctorIds = appointments.Select(a => a.DoctorId).Distinct().ToList();
        var doctors = await doctorRepository.ToListAsync(doctorRepository.Query().Where(d => doctorIds.Contains(d.Id)), cancellationToken);
        var doctorById = doctors.ToDictionary(d => d.Id);

        var byDoctor = appointments
            .GroupBy(a => a.DoctorId)
            .Select(group =>
            {
                decimal examRevenue = 0, serviceRevenue = 0;
                foreach (var appointment in group)
                {
                    if (!invoiceByAppointmentId.TryGetValue(appointment.Id, out var invoice)) continue;
                    if (!itemsByInvoiceId.TryGetValue(invoice.Id, out var items)) continue;

                    examRevenue += items.Where(i => i.MedicalRecordServiceId is null && i.MedicineId is null).Sum(i => i.Amount);
                    serviceRevenue += items.Where(i => i.MedicalRecordServiceId is not null).Sum(i => i.Amount);
                }

                return new DoctorReportRow(
                    group.Key,
                    doctorById.TryGetValue(group.Key, out var doctor) ? doctor.FullName : "",
                    group.Count(a => a.Status == AppointmentStatus.Completed),
                    examRevenue,
                    serviceRevenue,
                    group.Count(a => a.Status == AppointmentStatus.NoShow));
            })
            .OrderByDescending(r => r.VisitCount)
            .ToList();

        return new ClinicReportDto(from, to, totalVisits, totalRevenue, outstandingAmount, noShowRate, byDoctor);
    }
}
