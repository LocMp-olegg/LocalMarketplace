namespace LocMp.Contracts.Orders;

public sealed record OrderCompletedEvent(
    Guid OrderId,
    Guid BuyerId,
    Guid SellerId,
    Guid? CourierId,
    IReadOnlyList<OrderedProductItem> Products,
    decimal TotalAmount,
    DateTimeOffset OccurredAt) : IIntegrationEvent;