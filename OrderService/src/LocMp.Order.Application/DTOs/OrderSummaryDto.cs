using LocMp.Order.Domain.Enums;

namespace LocMp.Order.Application.DTOs;

public sealed record OrderSummaryDto(
    Guid Id,
    Guid BuyerId,
    Guid SellerId,
    OrderStatus Status,
    DeliveryType DeliveryType,
    PaymentStatus PaymentStatus,
    decimal TotalAmount,
    IReadOnlyList<OrderItemDto> Items,
    DateTimeOffset CreatedAt,
    DateTimeOffset? CompletedAt);