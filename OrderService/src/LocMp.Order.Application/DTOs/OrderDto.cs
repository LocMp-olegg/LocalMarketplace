using LocMp.Order.Domain.Enums;

namespace LocMp.Order.Application.DTOs;

public sealed record OrderDto(
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
    string? BuyerComment,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt,
    DateTimeOffset? CompletedAt,
    IReadOnlyList<OrderItemDto> Items,
    IReadOnlyList<OrderStatusHistoryDto> StatusHistory,
    IReadOnlyList<OrderPhotoDto> Photos,
    DeliveryAddressDto? DeliveryAddress,
    CourierAssignmentDto? CourierAssignment,
    DisputeDto? Dispute);
