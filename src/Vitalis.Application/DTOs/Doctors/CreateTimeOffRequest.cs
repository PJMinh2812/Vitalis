using Vitalis.Domain.Enums;

namespace Vitalis.Application.DTOs.Doctors;

// DoctorId == null means the whole clinic is closed for this period (VC-20).
public record CreateTimeOffRequest(
    int? DoctorId,
    DateTime StartAt,
    DateTime EndAt,
    TimeOffType Type,
    string? Reason);
