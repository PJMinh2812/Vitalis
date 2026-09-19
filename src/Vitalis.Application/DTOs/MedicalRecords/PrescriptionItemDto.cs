namespace Vitalis.Application.DTOs.MedicalRecords;

public record PrescriptionItemDto(
    int Id,
    int MedicineId,
    string? MedicineName,
    decimal? UnitPriceSnapshot,
    int Quantity,
    string? Dosage,
    int? DurationDays,
    string? Instruction);
