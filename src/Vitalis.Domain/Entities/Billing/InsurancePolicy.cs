namespace Vitalis.Domain.Entities.Billing;

public class InsurancePolicy : BaseEntity
{
    public int PatientId { get; set; }
    public string PolicyNumber { get; set; } = null!;
    public string? Provider { get; set; }
    public decimal? CoveragePercent { get; set; }
    public DateOnly? ValidFrom { get; set; }
    public DateOnly? ValidTo { get; set; }

    public Patient Patient { get; set; } = null!;
}
