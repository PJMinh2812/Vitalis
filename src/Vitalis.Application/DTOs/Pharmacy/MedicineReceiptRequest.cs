namespace Vitalis.Application.DTOs.Pharmacy;

// SupplierName/ReferenceCode are accepted but not persisted — the schema has no
// "receipt header" table, only medicine_batches + medicine_stock_transactions
// per line, so there's nowhere to store them yet.
public record ReceiptItemRequest(int MedicineId, string BatchNo, DateOnly ExpiryDate, int Quantity, decimal ImportPrice);

public record MedicineReceiptRequest(string? SupplierName, string? ReferenceCode, List<ReceiptItemRequest> Items);
