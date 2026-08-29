namespace Vitalis.Domain.Entities.Billing;

public class Invoice : AuditableEntity
{
    public string? InvoiceNo { get; set; }
    public int PatientId { get; set; }
    public int? AppointmentId { get; set; }
    public int? MedicalRecordId { get; set; }
    public string? PatientName { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal InsuranceAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public InvoiceStatus Status { get; set; } = InvoiceStatus.Unpaid;
    public DateTime? CancelledAt { get; set; }
    public string? CancelReason { get; set; }
    public int? CreatedBy { get; set; }

    public Patient Patient { get; set; } = null!;
    public Appointment? Appointment { get; set; }
    public MedicalRecord? MedicalRecord { get; set; }
    public User? CreatedByUser { get; set; }
    public ICollection<InvoiceItem> Items { get; set; } = new List<InvoiceItem>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
