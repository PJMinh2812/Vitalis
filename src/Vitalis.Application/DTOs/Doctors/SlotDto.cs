namespace Vitalis.Application.DTOs.Doctors;

public record SlotDto(TimeOnly StartTime, TimeOnly EndTime, bool IsAvailable);
