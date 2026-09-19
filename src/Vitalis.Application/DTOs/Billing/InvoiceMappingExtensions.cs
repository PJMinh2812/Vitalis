using Vitalis.Domain.Entities.Billing;

namespace Vitalis.Application.DTOs.Billing;

public static class InvoiceMappingExtensions
{
    public static InvoiceItemDto ToDto(this InvoiceItem item) => new(
        item.Id, item.MedicalRecordServiceId, item.MedicineId, item.Description, item.Quantity, item.UnitPrice, item.Amount);

    public static PaymentDto ToDto(this Payment payment) => new(
        payment.Id, payment.Amount, payment.Method, payment.ReferenceCode, payment.IsRefund, payment.PaidAt, payment.Note);

    // VC-06: "Còn lại = total_amount - insurance_amount - paid_amount" (discount is
    // tracked but no screen ever sets it — no endpoint writes discount_amount yet).
    public static InvoiceDto ToDto(this Invoice invoice, List<InvoiceItemDto> items, List<PaymentDto> payments) => new(
        invoice.Id,
        invoice.InvoiceNo,
        invoice.PatientId,
        invoice.PatientName,
        invoice.AppointmentId,
        invoice.MedicalRecordId,
        invoice.TotalAmount,
        invoice.InsuranceAmount,
        invoice.DiscountAmount,
        invoice.PaidAmount,
        invoice.TotalAmount - invoice.InsuranceAmount - invoice.PaidAmount,
        invoice.Status,
        invoice.CancelledAt is not null,
        items,
        payments,
        invoice.CreatedAt);
}
