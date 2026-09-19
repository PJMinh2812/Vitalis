namespace Vitalis.Application.DTOs.Medicines;

// VC-15: shown while a doctor is prescribing.
public record MedicineSearchDto(
    int Id,
    string Name,
    string? ActiveIngredient,
    string? Concentration,
    string? Unit,
    decimal Price,
    int AvailableStock,
    bool AllergyWarning);
