using System.Text.Json;
using LocMp.BuildingBlocks;
using NetTopologySuite.Geometries;

namespace LocMp.Catalog.Domain.Entities;

public class Product(Guid id) : AggregateRoot<Guid>(id)
{
    public Guid SellerId { get; set; }
    public Guid ShopId { get; set; }
    public Guid CategoryId { get; set; }

    public string Name { get; set; } = null!;
    public string? Description { get; set; }

    public decimal Price { get; set; }
    public string Unit { get; set; } = null!;

    public int StockQuantity { get; set; }

    public bool IsMadeToOrder { get; set; }
    public int? LeadTimeDays { get; set; }

    public Point? Location { get; set; }

    public JsonDocument? Attributes { get; set; }

    public decimal AverageRating { get; set; }
    public int ReviewCount { get; set; }

    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedAt { get; set; }

    public int Reserve(int quantity)
    {
        if (StockQuantity < quantity)
            throw new InvalidOperationException(
                $"Insufficient stock. Available: {StockQuantity}, requested: {quantity}.");
        StockQuantity -= quantity;
        UpdatedAt = DateTimeOffset.UtcNow;
        return StockQuantity;
    }

    public int Release(int quantity)
    {
        StockQuantity += quantity;
        UpdatedAt = DateTimeOffset.UtcNow;
        return StockQuantity;
    }

    public int AdjustStock(int delta)
    {
        var newQuantity = StockQuantity + delta;
        if (newQuantity < 0)
            throw new InvalidOperationException(
                $"Insufficient stock. Current: {StockQuantity}, delta: {delta}.");
        StockQuantity = newQuantity;
        UpdatedAt = DateTimeOffset.UtcNow;
        return newQuantity;
    }

    public virtual Shop Shop { get; set; } = null!;
    public virtual Category Category { get; set; } = null!;
    public virtual ICollection<ProductPhoto> Photos { get; set; } = [];
    public virtual ICollection<StockHistory> StockHistory { get; set; } = [];
    public virtual ICollection<ProductTag> ProductTags { get; set; } = [];
    public virtual ICollection<Favorite> Favorites { get; set; } = [];
}