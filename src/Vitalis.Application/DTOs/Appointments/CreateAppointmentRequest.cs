using Vitalis.Domain.Enums;

namespace Vitalis.Application.DTOs.Appointments;

// StartTime must land exactly on a slot boundary the client got from
// GET /api/doctors/{id}/available-slots — the service re-validates it against
// that same slot list, it never trusts a freeform time from the client.
public record CreateAppointmentRequest(
    int PatientId,
    int DoctorId,
    DateTime StartTime,
    string Reason,
    AppointmentSource Source);
