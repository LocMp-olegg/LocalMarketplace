using System.Net.Http.Json;

namespace LocMp.Order.Infrastructure.Clients;

public sealed class CatalogServiceClient(HttpClient http)
{
    public Task<ProductSnapshotDto?> GetProductAsync(Guid productId, CancellationToken ct = default)
        => http.GetFromJsonAsync<ProductSnapshotDto>($"api/products/{productId}", ct);

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