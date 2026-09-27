namespace Vitalis.Domain.Entities.Scheduling;

public class DoctorTimeOff : BaseEntity
{
    public int? DoctorId { get; set; }
    public TimeOffType Type { get; set; }
    public bool IsFullDay { get; set; } = true;
    public DateTime StartAt { get; set; }
    public DateTime EndAt { get; set; }
    public string? Reason { get; set; }
    public int? ApprovedBy { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Doctor? Doctor { get; set; }
    public User? ApprovedByUser { get; set; }
}
