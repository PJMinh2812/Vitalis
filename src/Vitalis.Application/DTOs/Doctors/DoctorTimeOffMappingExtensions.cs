using Vitalis.Domain.Entities.Scheduling;

namespace Vitalis.Application.DTOs.Doctors;

public static class DoctorTimeOffMappingExtensions
{
    public static DoctorTimeOffDto ToDto(this DoctorTimeOff timeOff) => new(
        timeOff.Id,
        timeOff.DoctorId,
        timeOff.Type,
        timeOff.IsFullDay,
        timeOff.StartAt,
        timeOff.EndAt,
        timeOff.Reason);
}
