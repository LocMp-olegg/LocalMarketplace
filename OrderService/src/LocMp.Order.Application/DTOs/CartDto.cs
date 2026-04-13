namespace LocMp.Order.Application.DTOs;

public sealed record CartDto(
    Guid Id,
    Guid UserId,
    DateTimeOffset CreatedAt,
    DateTimeOffset ExpiresAt,
    IReadOnlyList<CartItemDto> Items,
    decimal TotalAmount);