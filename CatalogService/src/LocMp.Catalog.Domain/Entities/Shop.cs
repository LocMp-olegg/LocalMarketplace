using LocMp.Catalog.Domain.Enums;
using NetTopologySuite.Geometries;

namespace LocMp.Catalog.Domain.Entities;

public class Shop(Guid id)
{
    public Guid Id { get; set; } = id;
    public Guid SellerId { get; set; }

    public string BusinessName { get; set; } = null!;
    public string PhoneNumber { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? Description { get; set; }

    public string? Inn { get; set; }
    public BusinessType BusinessType { get; set; } = BusinessType.Individual;

    public string? WorkingHours { get; set; }
    public int? ServiceRadiusMeters { get; set; }
    public Point? Location { get; set; }

    public string? AvatarUrl { get; set; }
    public string? AvatarObjectKey { get; set; }

    public bool IsVerified { get; set; } = true;
    public DateTimeOffset? VerifiedAt { get; set; }

    public bool IsActive { get; set; } = true;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedAt { get; set; }

    public virtual ICollection<Product> Products { get; set; } = [];
    public virtual ICollection<ShopPhoto> Photos { get; set; } = [];
}