using Vitalis.Domain.Entities.Scheduling;

namespace Vitalis.Application.DTOs.Doctors;

public static class SpecialtyMappingExtensions
{
    public static SpecialtyDto ToDto(this Specialty specialty) => new(
        specialty.Id,
        specialty.Code,
        specialty.Name,
        specialty.Description,
        specialty.IsActive);
}
