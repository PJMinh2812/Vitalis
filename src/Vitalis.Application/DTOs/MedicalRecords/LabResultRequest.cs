namespace Vitalis.Application.DTOs.MedicalRecords;

public record LabResultRequest(
    string ResultValue,
    string? ReferenceRange,
    string? Conclusion,
    DateTime ResultedAt);
