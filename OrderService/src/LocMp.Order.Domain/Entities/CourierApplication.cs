using LocMp.BuildingBlocks;
using LocMp.Order.Domain.Enums;
using NetTopologySuite.Geometries;

namespace LocMp.Order.Domain.Entities;

public sealed class CourierApplication(Guid id) : Entity<Guid>(id)
{
    public Guid OrderId { get; set; }
    public Guid CourierId { get; set; }
    public string CourierName { get; set; } = null!;
    public string CourierPhone { get; set; } = null!;
    public Point? CourierLocation { get; set; }
    public double? DistanceToShopMeters { get; set; }
    public CourierApplicationStatus Status { get; set; } = CourierApplicationStatus.Pending;
    public DateTimeOffset AppliedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedAt { get; set; }

    public Order Order { get; set; } = null!;
}