namespace Vitalis.Domain.Entities.Clinical;

public class MedicineBatch : BaseEntity
{
    public int MedicineId { get; set; }
    public string BatchNo { get; set; } = null!;
    public DateOnly ExpiryDate { get; set; }
    public int Quantity { get; set; }
    public decimal? ImportPrice { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Medicine Medicine { get; set; } = null!;
}
