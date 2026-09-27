using Vitalis.Domain.Enums;

namespace Vitalis.Application.DTOs.Billing;

public record InvoiceItemDto(
    int Id,
    int? MedicalRecordServiceId,
    int? MedicineId,
    string? Description,
    int Quantity,
    decimal UnitPrice,
    decimal Amount);

public record PaymentDto(
    int Id,
    decimal Amount,
    PaymentMethod Method,
    string? ReferenceCode,
    bool IsRefund,
    DateTime PaidAt,
    string? Note);

public record InvoiceDto(
    int Id,
    string? InvoiceNo,
    int PatientId,
    string? PatientName,
    int? AppointmentId,
    int? MedicalRecordId,
    decimal TotalAmount,
    decimal InsuranceAmount,
    decimal DiscountAmount,
    decimal PaidAmount,
    decimal Remaining,
    InvoiceStatus Status,
    bool IsCancelled,
    List<InvoiceItemDto> Items,
    List<PaymentDto> Payments,
    DateTime CreatedAt);
