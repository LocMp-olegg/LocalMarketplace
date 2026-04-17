namespace LocMp.Contracts.Orders;

public sealed record OrderedProductItem(
    Guid ProductId,
    string ProductName,
    int Quantity,
    decimal Subtotal,
    Guid? ShopId,
    string? ShopName);
