using Vitalis.Domain.Enums;

namespace Vitalis.Application.DTOs.Doctors;

public record DoctorTimeOffDto(
    int Id,
    int? DoctorId,
    TimeOffType Type,
    bool IsFullDay,
    DateTime StartAt,
    DateTime EndAt,
    string? Reason);
