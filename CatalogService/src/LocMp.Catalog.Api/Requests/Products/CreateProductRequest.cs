namespace LocMp.Catalog.Api.Requests.Products;

public sealed record CreateProductRequest(
    Guid CategoryId,
    string Name,
    string? Description,
    decimal Price,
    string Unit,
    int InitialStock,
    double? Latitude,
    double? Longitude
);
