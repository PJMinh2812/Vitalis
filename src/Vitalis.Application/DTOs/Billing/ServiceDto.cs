namespace Vitalis.Application.DTOs.Billing;

public record ServiceDto(
    int Id,
    string? Code,
    int? SpecialtyId,
    string Name,
    string? Description,
    decimal Price,
    int? DurationMinutes,
    bool IsActive);
