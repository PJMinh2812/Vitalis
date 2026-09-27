namespace Vitalis.Application.DTOs.Doctors;

public record CreateSpecialtyRequest(
    string? Code,
    string Name,
    string? Description);
