namespace Vitalis.Application.DTOs.Medicines;

public record MedicineDto(
    int Id,
    string? Code,
    string Name,
    string? ActiveIngredient,
    string? Concentration,
    string? Unit,
    decimal Price,
    int StockQuantity,
    bool IsActive);
