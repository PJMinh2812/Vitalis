using Vitalis.Application.DTOs.Medicines;
using Vitalis.Application.DTOs.Pharmacy;

namespace Vitalis.Application.Interfaces;

public interface IPharmacyService
{
    // VC-17 #1
    Task<IReadOnlyList<MedicineInventoryDto>> GetInventoryAsync(bool? lowStock, bool? nearExpiry, CancellationToken cancellationToken = default);

    // VC-18 #2
    Task<IReadOnlyList<MedicineDto>> CreateReceiptAsync(MedicineReceiptRequest request, int receivedBy, CancellationToken cancellationToken = default);

    // VC-16 #1 — deliberately ignores any client-supplied batch selection and
    // always allocates FEFO server-side (see learning-notes/13); rejects with
    // ConflictException if any line can't be fully covered (no negative stock).
    Task<DispenseResultDto> DispenseAsync(int prescriptionId, int dispensedBy, CancellationToken cancellationToken = default);

    // VC-17 #3 — write-off only (decreases the batch).
    Task AdjustStockAsync(int medicineId, AdjustStockRequest request, int adjustedBy, CancellationToken cancellationToken = default);

    // VC-17 #4 — "thẻ kho"
    Task<IReadOnlyList<StockTransactionDto>> GetTransactionsAsync(int medicineId, DateTime? from, DateTime? to, CancellationToken cancellationToken = default);
}
