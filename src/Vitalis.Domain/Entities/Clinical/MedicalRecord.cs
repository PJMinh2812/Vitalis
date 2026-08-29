namespace Vitalis.Domain.Entities.Clinical;

public class MedicalRecord : AuditableEntity
{
    public int AppointmentId { get; set; }
    public int PatientId { get; set; }
    public int DoctorId { get; set; }
    public string? Symptoms { get; set; }
    public string? Diagnosis { get; set; }
    public string? Icd10Code { get; set; }
    public string? TreatmentPlan { get; set; }
    public DateOnly? FollowUpDate { get; set; }
    public string? Note { get; set; }
    public MedicalRecordStatus Status { get; set; } = MedicalRecordStatus.Draft;
    public DateTime? FinalizedAt { get; set; }

    public Appointment Appointment { get; set; } = null!;
    public Patient Patient { get; set; } = null!;
    public Doctor Doctor { get; set; } = null!;
    public PatientVitals? Vitals { get; set; }
    public Prescription? Prescription { get; set; }
    public ICollection<MedicalRecordService> Services { get; set; } = new List<MedicalRecordService>();
    public ICollection<Attachment> Attachments { get; set; } = new List<Attachment>();
}
