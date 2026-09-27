using Vitalis.Domain.Entities.Scheduling;

namespace Vitalis.Application.DTOs.Appointments;

// PatientName/DoctorName are resolved by the caller (in-memory join in
// AppointmentService, same pattern as UserService/DoctorService) — no EF Core
// Include in Application.
public static class AppointmentMappingExtensions
{
    public static AppointmentDto ToDto(this Appointment appointment, string patientName, string doctorName) => new(
        appointment.Id,
        appointment.AppointmentCode,
        appointment.PatientId,
        patientName,
        appointment.DoctorId,
        doctorName,
        appointment.StartTime,
        appointment.EndTime,
        appointment.Status,
        appointment.Source,
        appointment.QueueNumber,
        appointment.FeeSnapshot,
        appointment.Reason,
        appointment.CancelReason,
        appointment.Note,
        appointment.CheckedInAt,
        appointment.CompletedAt,
        appointment.CreatedAt);
}
