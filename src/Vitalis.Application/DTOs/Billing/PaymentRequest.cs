using Vitalis.Domain.Enums;

namespace Vitalis.Application.DTOs.Billing;

// Same endpoint handles both a normal payment (IsRefund = false, Amount > 0)
// and a refund (IsRefund = true, Amount < 0) — see InvoiceService.AddPaymentAsync.
public record PaymentRequest(
    decimal Amount,
    PaymentMethod Method,
    string? ReferenceCode,
    string? Note,
    bool IsRefund = false);
