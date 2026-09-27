namespace Vitalis.Domain.Entities.Scheduling;

public class Patient : AuditableEntity
{
    public int? UserId { get; set; }
    public string? PatientCode { get; set; }
    public string FullName { get; set; } = null!;
    public Gender? Gender { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? NationalId { get; set; }
    public string? InsuranceNumber { get; set; }
    public string? BloodType { get; set; }
    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactPhone { get; set; }
    public bool IsActive { get; set; } = true;

    public User? User { get; set; }
    public ICollection<PatientAllergy> Allergies { get; set; } = new List<PatientAllergy>();
    public ICollection<InsurancePolicy> InsurancePolicies { get; set; } = new List<InsurancePolicy>();
}
