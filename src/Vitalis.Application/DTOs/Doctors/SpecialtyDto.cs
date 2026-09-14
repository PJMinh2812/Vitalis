namespace Vitalis.Application.DTOs.Doctors;

public record SpecialtyDto(
    int Id,
    string? Code,
    string Name,
    string? Description,
    bool IsActive);
