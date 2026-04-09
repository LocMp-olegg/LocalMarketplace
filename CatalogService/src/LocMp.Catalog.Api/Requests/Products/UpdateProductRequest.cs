namespace LocMp.Catalog.Api.Requests.Products;

public sealed record UpdateProductRequest(
    Guid CategoryId,
    string Name,
    string? Description,
    decimal Price,
    string Unit,
    bool IsActive,
    double? Latitude,
    double? Longitude
);
