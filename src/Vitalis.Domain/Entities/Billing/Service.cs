namespace Vitalis.Domain.Entities.Billing;

public class Service : AuditableEntity
{
    public string? Code { get; set; }
    public int? SpecialtyId { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int? DurationMinutes { get; set; }
    public bool IsActive { get; set; } = true;

    public Specialty? Specialty { get; set; }
}
