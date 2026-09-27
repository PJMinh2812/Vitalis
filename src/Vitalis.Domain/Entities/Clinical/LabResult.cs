namespace Vitalis.Domain.Entities.Clinical;

public class LabResult : BaseEntity
{
    public int MedicalRecordServiceId { get; set; }
    public string? ResultValue { get; set; }
    public string? ReferenceRange { get; set; }
    public string? Conclusion { get; set; }
    public DateTime? ResultedAt { get; set; }

    public MedicalRecordService MedicalRecordService { get; set; } = null!;
}
