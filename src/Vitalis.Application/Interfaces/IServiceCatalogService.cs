using Vitalis.Application.DTOs.Billing;

namespace Vitalis.Application.Interfaces;

// "ServiceCatalog" (not "IServiceService") to avoid colliding with the
// Billing.Service entity name.
public interface IServiceCatalogService
{
    Task<IReadOnlyList<ServiceDto>> GetAllAsync(bool? activeOnly, CancellationToken cancellationToken = default);

    Task<ServiceDto> CreateAsync(CreateServiceRequest request, CancellationToken cancellationToken = default);

    // VC-21 #2 — soft delete only, invoice_items/medical_record_services keep referencing it.
    Task DeactivateAsync(int id, CancellationToken cancellationToken = default);
}
