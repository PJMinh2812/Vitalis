namespace Vitalis.Application.DTOs.Pharmacy;

// Write-off only (decreases the batch) — the schema's medicine_stock_transactions
// CHECK requires quantity > 0 with no separate sign, and the only listed screen
// for an *increase* is the receipt flow (VC-18). Correcting a count upward would
// go through a new receipt, not this endpoint.
public record AdjustStockRequest(int BatchId, int Quantity, string Reason);
