using LocMp.BuildingBlocks;
using LocMp.Contracts.Orders;
using LocMp.Order.Domain.Enums;

namespace LocMp.Order.Domain.Entities;

public class Dispute(Guid id) : Entity<Guid>(id)
{
    public Guid OrderId { get; set; }

    public Guid InitiatorId { get; set; }
    public DisputeType DisputeType { get; set; }
    public string Reason { get; set; } = null!;
    public DisputeStatus Status { get; set; } = DisputeStatus.Open;
    public DisputeOutcome? Outcome { get; set; }
    public string? Resolution { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? ResolvedAt { get; set; }

    public virtual Order Order { get; set; } = null!;
    public virtual ICollection<DisputePhoto> Photos { get; set; } = [];
}
