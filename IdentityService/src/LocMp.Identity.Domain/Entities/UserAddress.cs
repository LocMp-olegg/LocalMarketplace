using NetTopologySuite.Geometries;

namespace LocMp.Identity.Domain.Entities;

public class UserAddress
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    
    public string Title { get; set; } = string.Empty;
    
    public string City { get; set; } = string.Empty;
    public string Street { get; set; } = string.Empty;
    public string HouseNumber { get; set; } = string.Empty;
    public string? Apartment { get; set; }
    public string? Entrance { get; set; }
    public string? Floor { get; set; }   
    
    public Point Location { get; set; } = null!;

    public bool IsDefault { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public virtual ApplicationUser User { get; set; } = null!;
}