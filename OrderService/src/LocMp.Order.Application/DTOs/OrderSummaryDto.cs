using LocMp.Order.Domain.Enums;

namespace LocMp.Order.Application.DTOs;

public sealed record OrderSummaryDto(
    Guid Id,
    Guid? CheckoutId,
    Guid BuyerId,
    Guid SellerId,
    string SellerName,
    Guid? ShopId,
    string? ShopName,
    OrderStatus Status,
    DeliveryType DeliveryType,
    PaymentStatus PaymentStatus,
    decimal TotalAmount,
    IReadOnlyList<OrderItemDto> Items,
    DateTimeOffset CreatedAt,
    DateTimeOffset? CompletedAt);
