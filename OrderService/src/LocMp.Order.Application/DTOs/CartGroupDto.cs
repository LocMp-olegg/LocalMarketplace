namespace LocMp.Order.Application.DTOs;

public sealed record CartGroupDto(
    Guid SellerId,
    string SellerName,
    Guid? ShopId,
    string? ShopName,
    IReadOnlyList<CartItemDto> Items,
    decimal GroupTotal);
