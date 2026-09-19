namespace Vitalis.Application.DTOs.Medicines;

// Minimal catalog entry — per-lot stock (medicine_batches) and receiving stock
// (VC-17/VC-18) are out of scope here; StockQuantity is just the opening balance.
public record CreateMedicineRequest(
    string? Code,
    string Name,
    string? ActiveIngredient,
    string? Concentration,
    string? Unit,
    decimal Price,
    int StockQuantity);
