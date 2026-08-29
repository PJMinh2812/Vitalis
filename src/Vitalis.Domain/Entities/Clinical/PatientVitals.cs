namespace Vitalis.Domain.Entities.Clinical;

public class PatientVitals : BaseEntity
{
    public int MedicalRecordId { get; set; }
    public decimal? Temperature { get; set; }
    public int? Pulse { get; set; }
    public string? BloodPressure { get; set; }
    public decimal? Weight { get; set; }
    public decimal? Height { get; set; }

    public MedicalRecord MedicalRecord { get; set; } = null!;
}
