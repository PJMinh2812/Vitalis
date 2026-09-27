using Vitalis.Domain.Entities.Billing;

namespace Vitalis.Application.DTOs.Billing;

public static class ServiceMappingExtensions
{
    public static ServiceDto ToDto(this Service service) => new(
        service.Id, service.Code, service.SpecialtyId, service.Name, service.Description,
        service.Price, service.DurationMinutes, service.IsActive);
}
