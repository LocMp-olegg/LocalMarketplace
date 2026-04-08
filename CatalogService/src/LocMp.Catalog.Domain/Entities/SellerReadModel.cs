using LocMp.BuildingBlocks;
using NetTopologySuite.Geometries;

namespace LocMp.Catalog.Domain.Entities;

public class SellerReadModel(Guid id) : Entity<Guid>(id)
{
    public string DisplayName { get; set; } = null!;
    public string? AvatarUrl { get; set; }
    public decimal AverageRating { get; set; }
    public int ReviewCount { get; set; }
    public Point? Location { get; set; }
    public DateTimeOffset LastSyncedAt { get; set; } = DateTimeOffset.UtcNow;
}