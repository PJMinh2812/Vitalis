namespace Vitalis.Application.DTOs.MedicalRecords;

public record VitalsDto(
    decimal? Temperature,
    int? Pulse,
    string? BloodPressure,
    decimal? Weight,
    decimal? Height);
