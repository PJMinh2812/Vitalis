namespace Vitalis.Domain.Entities.Clinical;

public class Attachment : BaseEntity
{
    public int MedicalRecordId { get; set; }
    public string FileUrl { get; set; } = null!;
    public string? FileType { get; set; }
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    public MedicalRecord MedicalRecord { get; set; } = null!;
}
