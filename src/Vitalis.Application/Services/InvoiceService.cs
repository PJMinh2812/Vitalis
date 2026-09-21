using Vitalis.Application.DTOs.Billing;
using Vitalis.Application.Exceptions;
using Vitalis.Application.Interfaces;
using Vitalis.Domain.Entities.Billing;
using Vitalis.Domain.Enums;
using ValidationException = Vitalis.Application.Exceptions.ValidationException;

namespace Vitalis.Application.Services;

public class InvoiceService(IUnitOfWork unitOfWork) : IInvoiceService
{
    public async Task<InvoiceDto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var invoice = await unitOfWork.Repository<Invoice>().GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Invoice), id);

        return await BuildDtoAsync(invoice, cancellationToken);
    }

    public async Task<IReadOnlyList<InvoiceDto>> GetPatientInvoicesAsync(int patientId, CancellationToken cancellationToken = default)
    {
        var repository = unitOfWork.Repository<Invoice>();
        var invoices = await repository.ToListAsync(
            repository.Query().Where(i => i.PatientId == patientId).OrderByDescending(i => i.CreatedAt), cancellationToken);

        var result = new List<InvoiceDto>();
        foreach (var invoice in invoices)
            result.Add(await BuildDtoAsync(invoice, cancellationToken));

        return result;
    }

    public async Task<InvoiceDto> AddPaymentAsync(int invoiceId, PaymentRequest request, int receivedBy, CancellationToken cancellationToken = default)
    {
        var invoiceRepository = unitOfWork.Repository<Invoice>();
        var invoice = await invoiceRepository.GetByIdAsync(invoiceId, cancellationToken)
            ?? throw new NotFoundException(nameof(Invoice), invoiceId);

        if (invoice.CancelledAt is not null)
            throw new ConflictException("Hoá đơn đã huỷ, không thể thu/hoàn tiền");

        var remaining = invoice.TotalAmount - invoice.InsuranceAmount - invoice.PaidAmount;

        if (request.IsRefund)
        {
            if (request.Amount >= 0)
                throw new ValidationException("amount", "Số tiền hoàn phải là số âm");
            if (-request.Amount > invoice.PaidAmount)
                throw new ConflictException("Số tiền hoàn không được lớn hơn số đã thu");
        }
        else
        {
            if (request.Amount <= 0)
                throw new ValidationException("amount", "Số tiền thu phải lớn hơn 0");
            if (request.Amount > remaining)
                throw new ConflictException($"Số tiền thu vượt quá số còn lại ({remaining:N0})");
        }

        // Never edits a prior payment row — a refund is its own append-only negative
        // entry, per VC-11 ("Ghi bút toán âm, không sửa dòng thu cũ").
        await unitOfWork.Repository<Payment>().AddAsync(new Payment
        {
            InvoiceId = invoiceId,
            Amount = request.Amount,
            Method = request.Method,
            ReferenceCode = request.ReferenceCode,
            ReceivedBy = receivedBy,
            IsRefund = request.IsRefund,
            Note = request.Note,
        }, cancellationToken);

        // A positive payment or a negative refund both fold into PaidAmount the
        // same way — no separate "subtract" branch needed.
        invoice.PaidAmount += request.Amount;
        invoice.Status = invoice.PaidAmount <= 0
            ? InvoiceStatus.Unpaid
            : invoice.PaidAmount >= invoice.TotalAmount - invoice.InsuranceAmount
                ? InvoiceStatus.Paid
                : InvoiceStatus.PartiallyPaid;
        invoice.UpdatedAt = DateTime.UtcNow;
        invoiceRepository.Update(invoice);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return await BuildDtoAsync(invoice, cancellationToken);
    }

    public async Task<InvoiceDto> CancelAsync(int invoiceId, string cancelReason, CancellationToken cancellationToken = default)
    {
        var repository = unitOfWork.Repository<Invoice>();
        var invoice = await repository.GetByIdAsync(invoiceId, cancellationToken)
            ?? throw new NotFoundException(nameof(Invoice), invoiceId);

        if (invoice.CancelledAt is not null)
            throw new ConflictException("Hoá đơn đã huỷ trước đó");

        if (invoice.PaidAmount != 0)
            throw new ConflictException("Chỉ có thể huỷ hoá đơn chưa thu tiền");

        invoice.CancelledAt = DateTime.UtcNow;
        invoice.CancelReason = cancelReason;
        invoice.UpdatedAt = DateTime.UtcNow;
        repository.Update(invoice);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return await BuildDtoAsync(invoice, cancellationToken);
    }

    private async Task<InvoiceDto> BuildDtoAsync(Invoice invoice, CancellationToken cancellationToken)
    {
        var itemRepository = unitOfWork.Repository<InvoiceItem>();
        var items = await itemRepository.ToListAsync(itemRepository.Query().Where(i => i.InvoiceId == invoice.Id), cancellationToken);

        var paymentRepository = unitOfWork.Repository<Payment>();
        var payments = await paymentRepository.ToListAsync(
            paymentRepository.Query().Where(p => p.InvoiceId == invoice.Id).OrderBy(p => p.PaidAt), cancellationToken);

        return invoice.ToDto(items.Select(i => i.ToDto()).ToList(), payments.Select(p => p.ToDto()).ToList());
    }
}
