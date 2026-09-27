using Vitalis.Application.DTOs.Billing;

namespace Vitalis.Application.Interfaces;

public interface IInvoiceService
{
    Task<InvoiceDto> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    // VC-06
    Task<IReadOnlyList<InvoiceDto>> GetPatientInvoicesAsync(int patientId, CancellationToken cancellationToken = default);

    // VC-11 #1/#3 — same action for a payment (Amount > 0) and a refund
    // (Amount < 0, IsRefund = true); who's allowed to send IsRefund is an
    // authorization decision made by the Controller, not this service.
    Task<InvoiceDto> AddPaymentAsync(int invoiceId, PaymentRequest request, int receivedBy, CancellationToken cancellationToken = default);

    // VC-11 #4 — only while PaidAmount == 0.
    Task<InvoiceDto> CancelAsync(int invoiceId, string cancelReason, CancellationToken cancellationToken = default);
}
