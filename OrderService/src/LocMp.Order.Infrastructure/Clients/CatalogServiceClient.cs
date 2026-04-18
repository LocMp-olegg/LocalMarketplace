using System.Net.Http.Json;
using LocMp.Order.Infrastructure.DTOs;
using LocMp.Order.Infrastructure.Interfaces;

namespace LocMp.Order.Infrastructure.Clients;

public sealed class CatalogServiceClient(HttpClient http) : ICatalogClient
{
    public Task<ProductSnapshotDto?> GetProductAsync(Guid productId, CancellationToken ct = default)
        => http.GetFromJsonAsync<ProductSnapshotDto>($"api/products/{productId}", ct);

    public async Task<ShopDeliverySettingsDto?> GetShopDeliverySettingsAsync(Guid shopId, CancellationToken ct = default)
    {
        var shop = await http.GetFromJsonAsync<ShopSettingsResponse>($"api/shops/{shopId}", ct);
        return shop is null ? null : new ShopDeliverySettingsDto(shop.AllowCourierDelivery);
    }

    private sealed record ShopSettingsResponse(bool AllowCourierDelivery);

    public async Task ReserveStockAsync(Guid productId, int quantity, Guid orderId, CancellationToken ct = default)
    {
        var response = await http.PostAsJsonAsync(
            $"api/products/{productId}/reserve",
            new { quantity, orderId }, ct);
        response.EnsureSuccessStatusCode();
    }

    public async Task ReleaseStockAsync(Guid productId, int quantity, Guid orderId, CancellationToken ct = default)
    {
        var response = await http.PostAsJsonAsync(
            $"api/products/{productId}/release",
            new { quantity, orderId }, ct);
        response.EnsureSuccessStatusCode();
    }
}
