namespace Vitalis.Application.DTOs.Billing;

// VC-21 — full admin CRUD for the service catalog is step 9's scope; this
// minimal create is groundwork so VC-14 (order a service during a visit) has
// something to reference in step 8.
public record CreateServiceRequest(
    string? Code,
    int? SpecialtyId,
    string Name,
    string? Description,
    decimal Price,
    int? DurationMinutes);
