using FluentAssertions;
using Moq;
using Vitalis.Application.DTOs.Billing;
using Vitalis.Application.Exceptions;
using Vitalis.Application.Interfaces;
using Vitalis.Application.Services;
using Vitalis.Domain.Entities.Billing;
using Vitalis.Domain.Enums;

namespace Vitalis.Application.UnitTests.Services;

// CLAUDE.md: "Test tính tổng tiền hóa đơn" — covered here via payment/remaining/
// status calculation (AddPaymentAsync). The invoice-item aggregation itself
// (MedicalRecordsService.FinalizeAsync) was verified end-to-end against the
// real database instead of mocked here — mocking that many repositories for
// one arithmetic check wasn't worth it (see learning-notes/09).
public class InvoiceServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<IRepository<Invoice>> _invoiceRepository = new();
    private readonly Mock<IRepository<InvoiceItem>> _invoiceItemRepository = new();
    private readonly Mock<IRepository<Payment>> _paymentRepository = new();
    private readonly InvoiceService _sut;

    public InvoiceServiceTests()
    {
        _unitOfWork.Setup(u => u.Repository<Invoice>()).Returns(_invoiceRepository.Object);
        _unitOfWork.Setup(u => u.Repository<InvoiceItem>()).Returns(_invoiceItemRepository.Object);
        _unitOfWork.Setup(u => u.Repository<Payment>()).Returns(_paymentRepository.Object);
        _unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _invoiceItemRepository.Setup(r => r.ToListAsync(It.IsAny<IQueryable<InvoiceItem>>(), It.IsAny<CancellationToken>())).ReturnsAsync([]);
        _paymentRepository.Setup(r => r.ToListAsync(It.IsAny<IQueryable<Payment>>(), It.IsAny<CancellationToken>())).ReturnsAsync([]);

        _sut = new InvoiceService(_unitOfWork.Object);
    }

    [Fact]
    public async Task AddPaymentAsync_FullPayment_SetsStatusPaidAndZeroRemaining()
    {
        var invoice = new Invoice { Id = 1, TotalAmount = 320_000, InsuranceAmount = 0, PaidAmount = 0 };
        _invoiceRepository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(invoice);

        var result = await _sut.AddPaymentAsync(1, new PaymentRequest(320_000, PaymentMethod.Cash, null, null), receivedBy: 2);

        result.Status.Should().Be(InvoiceStatus.Paid);
        result.Remaining.Should().Be(0);
    }

    [Fact]
    public async Task AddPaymentAsync_PartialPayment_SetsStatusPartiallyPaidWithCorrectRemaining()
    {
        var invoice = new Invoice { Id = 1, TotalAmount = 320_000, InsuranceAmount = 0, PaidAmount = 0 };
        _invoiceRepository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(invoice);

        var result = await _sut.AddPaymentAsync(1, new PaymentRequest(100_000, PaymentMethod.Cash, null, null), receivedBy: 2);

        result.Status.Should().Be(InvoiceStatus.PartiallyPaid);
        result.Remaining.Should().Be(220_000);
    }

    [Fact]
    public async Task AddPaymentAsync_InsuranceReducesWhatCountsAsFullyPaid()
    {
        // total 320k, insurance covers 120k -> patient only owes 200k.
        var invoice = new Invoice { Id = 1, TotalAmount = 320_000, InsuranceAmount = 120_000, PaidAmount = 0 };
        _invoiceRepository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(invoice);

        var result = await _sut.AddPaymentAsync(1, new PaymentRequest(200_000, PaymentMethod.Cash, null, null), receivedBy: 2);

        result.Status.Should().Be(InvoiceStatus.Paid);
        result.Remaining.Should().Be(0);
    }

    [Fact]
    public async Task AddPaymentAsync_WhenAmountExceedsRemaining_ThrowsConflict()
    {
        var invoice = new Invoice { Id = 1, TotalAmount = 100_000, InsuranceAmount = 0, PaidAmount = 0 };
        _invoiceRepository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(invoice);

        var act = () => _sut.AddPaymentAsync(1, new PaymentRequest(150_000, PaymentMethod.Cash, null, null), receivedBy: 2);

        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task AddPaymentAsync_Refund_DecreasesPaidAmountAndReopensInvoice()
    {
        var invoice = new Invoice { Id = 1, TotalAmount = 320_000, InsuranceAmount = 0, PaidAmount = 320_000, Status = InvoiceStatus.Paid };
        _invoiceRepository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(invoice);

        var request = new PaymentRequest(-320_000, PaymentMethod.Cash, null, "Hoan tien do huy lich", IsRefund: true);
        var result = await _sut.AddPaymentAsync(1, request, receivedBy: 2);

        result.PaidAmount.Should().Be(0);
        result.Status.Should().Be(InvoiceStatus.Unpaid);
    }

    [Fact]
    public async Task AddPaymentAsync_WhenRefundExceedsPaidAmount_ThrowsConflict()
    {
        var invoice = new Invoice { Id = 1, TotalAmount = 320_000, InsuranceAmount = 0, PaidAmount = 100_000 };
        _invoiceRepository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(invoice);

        var request = new PaymentRequest(-200_000, PaymentMethod.Cash, null, null, IsRefund: true);
        var act = () => _sut.AddPaymentAsync(1, request, receivedBy: 2);

        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task AddPaymentAsync_WhenInvoiceIsCancelled_ThrowsConflict()
    {
        var invoice = new Invoice { Id = 1, TotalAmount = 100_000, CancelledAt = DateTime.UtcNow };
        _invoiceRepository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(invoice);

        var act = () => _sut.AddPaymentAsync(1, new PaymentRequest(50_000, PaymentMethod.Cash, null, null), receivedBy: 2);

        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task CancelAsync_WhenAlreadyPartiallyPaid_ThrowsConflict()
    {
        var invoice = new Invoice { Id = 1, PaidAmount = 50_000 };
        _invoiceRepository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(invoice);

        var act = () => _sut.CancelAsync(1, "Doi y");

        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task CancelAsync_WhenUnpaid_Succeeds()
    {
        var invoice = new Invoice { Id = 1, PaidAmount = 0 };
        _invoiceRepository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(invoice);

        var result = await _sut.CancelAsync(1, "Benh nhan khong den");

        result.IsCancelled.Should().BeTrue();
    }
}
