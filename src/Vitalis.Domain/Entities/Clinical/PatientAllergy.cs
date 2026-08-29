namespace Vitalis.Domain.Entities.Clinical;

public class PatientAllergy : BaseEntity
{
    public int PatientId { get; set; }
    public string Allergen { get; set; } = null!;
    public AllergySeverity? Severity { get; set; }
    public string? Note { get; set; }

    public Patient Patient { get; set; } = null!;
}
