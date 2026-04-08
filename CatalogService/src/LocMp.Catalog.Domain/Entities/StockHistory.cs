using LocMp.BuildingBlocks;
using LocMp.Catalog.Domain.Enums;

namespace LocMp.Catalog.Domain.Entities;

public class StockHistory(Guid id) : Entity<Guid>(id)
{
    public Guid ProductId { get; set; }

    public StockChangeType ChangeType { get; set; }
    public int QuantityDelta { get; set; }
    public int QuantityAfter { get; set; }
    public Guid? ReferenceId { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public virtual Product Product { get; set; } = null!;
}