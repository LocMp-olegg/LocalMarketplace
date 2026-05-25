using NetTopologySuite.Geometries;

namespace LocMp.Identity.Domain.Entities;

public class CourierProfile(Guid courierId)
{
    public Guid CourierId { get; set; } = courierId;
    public bool IsActive { get; set; }
    public int ServiceRadiusMeters { get; set; } = 1000;
    public Point? BaseLocation { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedAt { get; set; }

    public virtual ApplicationUser Courier { get; set; } = null!;
}