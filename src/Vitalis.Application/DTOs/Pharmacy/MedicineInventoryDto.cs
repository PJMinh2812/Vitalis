namespace Vitalis.Application.DTOs.Pharmacy;

// VC-17: TotalStock is SUM(batches.quantity) over non-expired batches — the
// live source of truth, not the denormalized medicines.stock_quantity column.
public record MedicineInventoryDto(
    int Id,
    string? Code,
    string Name,
    string? ActiveIngredient,
    int TotalStock,
    int? MinStock,
    bool LowStock,
    DateOnly? NearestExpiry,
    bool NearExpiry,
    bool IsActive);
