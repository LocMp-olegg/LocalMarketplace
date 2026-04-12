using LocMp.Order.Domain.Enums;

namespace LocMp.Order.Application.DTOs;

public sealed record OrderStatusHistoryDto(
    Guid Id,
    OrderStatus? FromStatus,
    OrderStatus ToStatus,
    string? Comment,
    Guid ChangedById,
    DateTimeOffset ChangedAt);
