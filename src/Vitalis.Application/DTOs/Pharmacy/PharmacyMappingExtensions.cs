using Vitalis.Domain.Entities.Clinical;

namespace Vitalis.Application.DTOs.Pharmacy;

public static class PharmacyMappingExtensions
{
    public static StockTransactionDto ToDto(this MedicineStockTransaction transaction) => new(
        transaction.Id, transaction.BatchId, transaction.Type, transaction.Quantity, transaction.PrescriptionId, transaction.CreatedAt);
}
