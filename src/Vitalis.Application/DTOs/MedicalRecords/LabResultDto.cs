namespace Vitalis.Application.DTOs.MedicalRecords;

public record LabResultDto(
    int Id,
    string? ResultValue,
    string? ReferenceRange,
    string? Conclusion,
    DateTime? ResultedAt);
