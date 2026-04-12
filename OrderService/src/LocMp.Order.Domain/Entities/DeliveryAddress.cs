using LocMp.BuildingBlocks;
using NetTopologySuite.Geometries;

namespace LocMp.Order.Domain.Entities;

public class DeliveryAddress(Guid id) : Entity<Guid>(id)
{
    public Guid OrderId { get; set; }

    public string City { get; set; } = null!;
    public string Street { get; set; } = null!;
    public string HouseNumber { get; set; } = null!;
    public string? Apartment { get; set; }
    public string? Entrance { get; set; }
    public int? Floor { get; set; }
    public Point? Location { get; set; }

    public string RecipientName { get; set; } = null!;
    public string RecipientPhone { get; set; } = null!;

    public virtual Order Order { get; set; } = null!;
}