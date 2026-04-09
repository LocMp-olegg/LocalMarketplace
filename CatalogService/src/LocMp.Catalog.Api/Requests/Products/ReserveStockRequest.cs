namespace LocMp.Catalog.Api.Requests.Products;

public sealed record ReserveStockRequest(int Quantity, Guid OrderId);
