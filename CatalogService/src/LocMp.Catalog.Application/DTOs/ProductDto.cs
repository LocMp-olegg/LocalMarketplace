namespace LocMp.Catalog.Application.DTOs;

public sealed record ProductDto
{
    public Guid Id { get; init; }
    public Guid SellerId { get; init; }
    public Guid? ShopId { get; init; }
    public string? ShopName { get; init; }
    public string? SellerName { get; init; }
    public Guid CategoryId { get; init; }
    public string Name { get; init; } = null!;
    public string? Description { get; init; }
    public decimal Price { get; init; }
    public string Unit { get; init; } = null!;
    public int StockQuantity { get; init; }
    public bool IsMadeToOrder { get; init; }
    public int? LeadTimeDays { get; init; }
    public double? Latitude { get; init; }
    public double? Longitude { get; init; }
    public bool IsActive { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? UpdatedAt { get; init; }
    public string? MainPhotoUrl { get; init; }
    public IReadOnlyList<ProductPhotoDto> Photos { get; init; } = [];
    public IReadOnlyList<string> Tags { get; init; } = [];
}