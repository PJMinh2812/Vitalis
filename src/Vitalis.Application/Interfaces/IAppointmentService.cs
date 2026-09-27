using Vitalis.Application.Common;
using Vitalis.Application.DTOs.Appointments;
using Vitalis.Domain.Enums;

namespace Vitalis.Application.Interfaces;

public interface IAppointmentService
{
    // VC-03/VC-09: books the exact slot, re-validated against the doctor's
    // current available slots — throws SlotUnavailableException (409) if it was
    // just taken, either by the pre-check or by the unique index as a backstop.
    Task<AppointmentDto> BookAsync(CreateAppointmentRequest request, int? createdBy, CancellationToken cancellationToken = default);

    Task<PagedResult<AppointmentDto>> SearchAsync(int? patientId, int? doctorId, DateOnly? date, AppointmentStatus? status, int page, int pageSize, CancellationToken cancellationToken = default);

    Task<AppointmentDto> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    // VC-04: blocked if already Completed/Cancelled/NoShow, or too close to start time.
    Task<AppointmentDto> CancelAsync(int id, string cancelReason, int? changedBy, CancellationToken cancellationToken = default);

    // VC-04: re-validates the new slot the same way BookAsync does.
    Task<AppointmentDto> RescheduleAsync(int id, DateTime newStartTime, int? changedBy, CancellationToken cancellationToken = default);

    // VC-07: assigns the next queue number for that doctor/day.
    Task<AppointmentDto> CheckInAsync(int id, int? changedBy, CancellationToken cancellationToken = default);

    Task<AppointmentDto> MarkNoShowAsync(int id, string? note, int? changedBy, CancellationToken cancellationToken = default);
}
