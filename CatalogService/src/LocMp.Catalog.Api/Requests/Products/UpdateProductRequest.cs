namespace LocMp.Catalog.Api.Requests.Products;

public sealed record UpdateProductRequest(
    Guid CategoryId,
    string Name,
    string? Description,
    decimal Price,
    string Unit,
    bool IsActive,
    bool IsMadeToOrder,
    int? LeadTimeDays,
    double? Latitude,
    double? Longitude
);
