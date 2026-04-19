namespace LocMp.Catalog.Api.Requests.Products;

public sealed record CreateProductRequest(
    Guid ShopId,
    Guid CategoryId,
    string Name,
    string? Description,
    decimal Price,
    string Unit,
    int InitialStock,
    bool IsMadeToOrder,
    int? LeadTimeDays,
    double? Latitude,
    double? Longitude
);
