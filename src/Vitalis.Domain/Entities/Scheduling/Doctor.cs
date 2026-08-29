namespace Vitalis.Domain.Entities.Scheduling;

public class Doctor : AuditableEntity
{
    public int UserId { get; set; }
    public int SpecialtyId { get; set; }
    public string FullName { get; set; } = null!;
    public string? Title { get; set; }
    public string? LicenseNumber { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Room { get; set; }
    public decimal? ConsultationFee { get; set; }
    public string? Bio { get; set; }
    public string? AvatarUrl { get; set; }
    public int? ExperienceYears { get; set; }
    public int? MaxPatientsPerDay { get; set; }
    public bool IsActive { get; set; } = true;

    public User User { get; set; } = null!;
    public Specialty Specialty { get; set; } = null!;
    public ICollection<DoctorSchedule> Schedules { get; set; } = new List<DoctorSchedule>();
}
