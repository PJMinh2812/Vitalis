namespace Vitalis.Domain.Entities.Billing;

public class Payment : BaseEntity
{
    public int InvoiceId { get; set; }
    public decimal Amount { get; set; }
    public PaymentMethod Method { get; set; } = PaymentMethod.Cash;
    public string? ReferenceCode { get; set; }
    public int? ReceivedBy { get; set; }
    public bool IsRefund { get; set; }
    public DateTime PaidAt { get; set; } = DateTime.UtcNow;
    public string? Note { get; set; }

    public Invoice Invoice { get; set; } = null!;
    public User? ReceivedByUser { get; set; }
}
