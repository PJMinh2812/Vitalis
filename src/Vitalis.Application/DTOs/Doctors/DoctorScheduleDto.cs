namespace Vitalis.Application.DTOs.Doctors;

public record DoctorScheduleDto(
    int Id,
    int DoctorId,
    byte DayOfWeek,
    TimeOnly StartTime,
    TimeOnly EndTime,
    TimeOnly? BreakStart,
    TimeOnly? BreakEnd,
    int SlotMinutes,
    DateOnly EffectiveFrom,
    DateOnly? EffectiveTo,
    bool IsActive);
