using Vitalis.Domain.Entities.Scheduling;

namespace Vitalis.Application.DTOs.Doctors;

public static class DoctorScheduleMappingExtensions
{
    public static DoctorScheduleDto ToDto(this DoctorSchedule schedule) => new(
        schedule.Id,
        schedule.DoctorId,
        (byte)schedule.DayOfWeek,
        schedule.StartTime,
        schedule.EndTime,
        schedule.BreakStart,
        schedule.BreakEnd,
        schedule.SlotMinutes,
        schedule.EffectiveFrom,
        schedule.EffectiveTo,
        schedule.IsActive);
}
