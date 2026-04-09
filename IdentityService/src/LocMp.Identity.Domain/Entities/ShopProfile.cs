using LocMp.Identity.Domain.Enums;
using NetTopologySuite.Geometries;

namespace LocMp.Identity.Domain.Entities;

//TODO: перенести в другой сервис
public class ShopProfile
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }

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
    public Guid? VerifiedByAdminId { get; set; }

    public bool IsActive { get; set; } = true;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedAt { get; set; }

    public virtual ApplicationUser User { get; set; } = null!;
}
