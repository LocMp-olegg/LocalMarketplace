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
    int ItemCount,
    string? FirstItemName,
    string? FirstItemPhotoUrl,
    DateTimeOffset CreatedAt,
    DateTimeOffset? CompletedAt);
