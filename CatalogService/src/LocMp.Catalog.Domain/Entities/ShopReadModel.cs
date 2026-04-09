namespace LocMp.Catalog.Domain.Entities;

public class ShopReadModel(Guid id)
{
    public Guid Id { get; set; } = id;
    public Guid SellerId { get; set; }
    public string BusinessName { get; set; } = null!;
    public string? Description { get; set; }
    public string? WorkingHours { get; set; }
    public int? ServiceRadiusMeters { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTimeOffset LastSyncedAt { get; set; } = DateTimeOffset.UtcNow;
}
