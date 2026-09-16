using Vitalis.Domain.Enums;

namespace Vitalis.Application.DTOs.Appointments;

public record AppointmentDto(
    int Id,
    string? AppointmentCode,
    int PatientId,
    string PatientName,
    int DoctorId,
    string DoctorName,
    DateTime StartTime,
    DateTime EndTime,
    AppointmentStatus Status,
    AppointmentSource Source,
    int? QueueNumber,
    decimal? FeeSnapshot,
    string? Reason,
    string? CancelReason,
    string? Note,
    DateTime? CheckedInAt,
    DateTime? CompletedAt,
    DateTime CreatedAt);
