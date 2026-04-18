using LocMp.Order.Infrastructure.DTOs;

namespace LocMp.Order.Infrastructure.Interfaces;

public interface ICatalogClient
{
    Task<ProductSnapshotDto?> GetProductAsync(Guid productId, CancellationToken ct = default);
    Task<ShopDeliverySettingsDto?> GetShopDeliverySettingsAsync(Guid shopId, CancellationToken ct = default);
    Task ReserveStockAsync(Guid productId, int quantity, Guid orderId, CancellationToken ct = default);
    Task ReleaseStockAsync(Guid productId, int quantity, Guid orderId, CancellationToken ct = default);
}