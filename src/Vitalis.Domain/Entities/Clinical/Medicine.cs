namespace Vitalis.Domain.Entities.Clinical;

public class Medicine : AuditableEntity
{
    public string? Code { get; set; }
    public string Name { get; set; } = null!;
    public string? ActiveIngredient { get; set; }
    public string? Concentration { get; set; }
    public string? Unit { get; set; }
    public decimal Price { get; set; }
    public decimal? CostPrice { get; set; }
    public int StockQuantity { get; set; }
    public int? MinStock { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<MedicineBatch> Batches { get; set; } = new List<MedicineBatch>();
}
