namespace Vitalis.Domain.Entities.Clinical;

public class Prescription : BaseEntity
{
    public int MedicalRecordId { get; set; }
    public int DoctorId { get; set; }
    public PrescriptionStatus Status { get; set; } = PrescriptionStatus.Pending;
    public DateTime? DispensedAt { get; set; }
    public int? DispensedBy { get; set; }
    public string? Note { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public MedicalRecord MedicalRecord { get; set; } = null!;
    public Doctor Doctor { get; set; } = null!;
    public User? DispensedByUser { get; set; }
    public ICollection<PrescriptionItem> Items { get; set; } = new List<PrescriptionItem>();
}
