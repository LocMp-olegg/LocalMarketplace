using LocMp.BuildingBlocks;

namespace LocMp.Order.Domain.Entities;

public class CourierAssignment(Guid id) : Entity<Guid>(id)
{
    public Guid OrderId { get; set; }

    public Guid CourierId { get; set; }
    public string CourierName { get; set; } = null!;
    public string CourierPhone { get; set; } = null!;

    public DateTimeOffset AssignedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? PickedUpAt { get; set; }
    public DateTimeOffset? DeliveredAt { get; set; }

    public virtual Order Order { get; set; } = null!;
}