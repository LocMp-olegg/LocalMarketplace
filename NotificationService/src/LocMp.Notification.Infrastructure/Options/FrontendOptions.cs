namespace LocMp.Notification.Infrastructure.Options;

public sealed class FrontendOptions
{
    private string BaseUrl { get; init; } = "http://localhost:5713";

    public string OrderUrl(Guid orderId) => $"{BaseUrl}/orders/{orderId}";
    public string SellerOrdersUrl() => $"{BaseUrl}/seller/orders";
    public string SellerAnalyticsUrl() => $"{BaseUrl}/seller/analytics";
    public string ProductUrl(Guid productId) => $"{BaseUrl}/product/{productId}";
    public string ProductEditUrl(Guid productId) => $"{BaseUrl}/seller/products/{productId}/edit";
    public string SellerShopsUrl() => $"{BaseUrl}/seller/shops";
}