namespace Vitalis.Domain.Entities.Scheduling;

public class AppointmentStatusHistory : BaseEntity
{
    public int AppointmentId { get; set; }
    public AppointmentStatus? FromStatus { get; set; }
    public AppointmentStatus ToStatus { get; set; }
    public int? ChangedBy { get; set; }
    public string? Reason { get; set; }
    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;

    public Appointment Appointment { get; set; } = null!;
    public User? ChangedByUser { get; set; }
}
