namespace LocMp.Contracts.Orders;

public sealed record OrderCompletedEvent(
    Guid OrderId,
    Guid BuyerId,
    Guid SellerId,
    string SellerName,
    Guid? CourierId,
    IReadOnlyList<OrderedProductItem> Products,
    decimal TotalAmount,
    DateTimeOffset OccurredAt) : IIntegrationEvent;