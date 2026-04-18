namespace LocMp.Order.Application.DTOs;

public sealed record CartItemDto(
    Guid Id,
    Guid ProductId,
    string ProductName,
    Guid SellerId,
    string SellerName,
    Guid? ShopId,
    string? ShopName,
    decimal Price,
    int Quantity,
    decimal Subtotal);
