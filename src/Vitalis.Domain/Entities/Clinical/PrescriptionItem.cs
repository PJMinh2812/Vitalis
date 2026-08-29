namespace Vitalis.Domain.Entities.Clinical;

public class PrescriptionItem : BaseEntity
{
    public int PrescriptionId { get; set; }
    public int MedicineId { get; set; }
    public string? MedicineNameSnapshot { get; set; }
    public decimal? UnitPriceSnapshot { get; set; }
    public int Quantity { get; set; } = 1;
    public string? Dosage { get; set; }
    public int? DurationDays { get; set; }
    public string? Frequency { get; set; }
    public string? Instruction { get; set; }

    public Prescription Prescription { get; set; } = null!;
    public Medicine Medicine { get; set; } = null!;
}
