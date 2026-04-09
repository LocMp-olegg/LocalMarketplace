using LocMp.Catalog.Domain.Enums;

namespace LocMp.Catalog.Api.Requests.Products;

public sealed record UpdateStockRequest(
    int QuantityDelta,
    StockChangeType ChangeType,
    Guid? ReferenceId = null
);
