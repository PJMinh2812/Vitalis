namespace Vitalis.Application.DTOs.Doctors;

// VC-20: creating time-off never blocks on conflicts, it just reports the
// appointments reception now needs to reschedule.
public record ConflictingAppointmentDto(
    int Id,
    string? AppointmentCode,
    DateTime StartTime,
    DateTime EndTime,
    string PatientName);

public record CreateTimeOffResult(
    DoctorTimeOffDto TimeOff,
    IReadOnlyList<ConflictingAppointmentDto> ConflictingAppointments);
