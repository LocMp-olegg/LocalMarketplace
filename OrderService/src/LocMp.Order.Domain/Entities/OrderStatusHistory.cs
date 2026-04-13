using LocMp.BuildingBlocks;
using LocMp.Order.Domain.Enums;

namespace LocMp.Order.Domain.Entities;

public class OrderStatusHistory(Guid id) : Entity<Guid>(id)
{
    public Guid OrderId { get; set; }

    public OrderStatus? FromStatus { get; set; }
    public OrderStatus ToStatus { get; set; }
    public string? Comment { get; set; }
    public Guid ChangedById { get; set; }
    public DateTimeOffset ChangedAt { get; set; } = DateTimeOffset.UtcNow;

    public virtual Order Order { get; set; } = null!;
}
