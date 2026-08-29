namespace Vitalis.Domain.Entities.Clinical;

public class MedicalRecordService : BaseEntity
{
    public int MedicalRecordId { get; set; }
    public int ServiceId { get; set; }
    public int Quantity { get; set; } = 1;
    public decimal UnitPriceSnapshot { get; set; }
    public MedicalRecordServiceStatus Status { get; set; } = MedicalRecordServiceStatus.Ordered;
    public int? PerformedBy { get; set; }
    public DateTime OrderedAt { get; set; } = DateTime.UtcNow;

    public MedicalRecord MedicalRecord { get; set; } = null!;
    public Service Service { get; set; } = null!;
    public User? PerformedByUser { get; set; }
    public ICollection<LabResult> LabResults { get; set; } = new List<LabResult>();
}
