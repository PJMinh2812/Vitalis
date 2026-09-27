namespace Vitalis.Domain.Entities.Clinical;

public class MedicineStockTransaction : BaseEntity
{
    public int MedicineId { get; set; }
    public int BatchId { get; set; }
    public StockTransactionType Type { get; set; }
    public int Quantity { get; set; }
    public int? PrescriptionId { get; set; }
    public int CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Medicine Medicine { get; set; } = null!;
    public MedicineBatch Batch { get; set; } = null!;
    public Prescription? Prescription { get; set; }
    public User CreatedByUser { get; set; } = null!;
}
