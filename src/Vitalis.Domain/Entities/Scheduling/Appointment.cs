namespace Vitalis.Domain.Entities.Scheduling;

public class Appointment : AuditableEntity
{
    public string? AppointmentCode { get; set; }
    public int PatientId { get; set; }
    public int DoctorId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public AppointmentStatus Status { get; set; } = AppointmentStatus.Pending;
    public AppointmentSource Source { get; set; } = AppointmentSource.Online;
    public int? QueueNumber { get; set; }
    public decimal? FeeSnapshot { get; set; }
    public string? Reason { get; set; }
    public string? CancelReason { get; set; }
    public string? Note { get; set; }
    public DateTime? CheckedInAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int? CreatedBy { get; set; }

    public Patient Patient { get; set; } = null!;
    public Doctor Doctor { get; set; } = null!;
    public User? CreatedByUser { get; set; }
    public ICollection<AppointmentStatusHistory> StatusHistory { get; set; } = new List<AppointmentStatusHistory>();
}
