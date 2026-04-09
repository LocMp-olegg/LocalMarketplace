namespace LocMp.Catalog.Api.Requests.Products;

public sealed record ReleaseStockRequest(int Quantity, Guid OrderId);
