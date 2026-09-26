using Vitalis.Domain.Enums;

namespace Vitalis.Application.DTOs.Pharmacy;

public record StockTransactionDto(
    int Id,
    int BatchId,
    StockTransactionType Type,
    int Quantity,
    int? PrescriptionId,
    DateTime CreatedAt);
