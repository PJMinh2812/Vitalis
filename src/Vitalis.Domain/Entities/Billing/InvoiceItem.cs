namespace Vitalis.Domain.Entities.Billing;

public class InvoiceItem : BaseEntity
{
    public int InvoiceId { get; set; }
    public int? MedicalRecordServiceId { get; set; }
    public int? MedicineId { get; set; }
    public string? Description { get; set; }
    public int Quantity { get; set; } = 1;
    public decimal UnitPrice { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal Amount { get; set; }

    public Invoice Invoice { get; set; } = null!;
    public MedicalRecordService? MedicalRecordService { get; set; }
    public Medicine? Medicine { get; set; }
}
