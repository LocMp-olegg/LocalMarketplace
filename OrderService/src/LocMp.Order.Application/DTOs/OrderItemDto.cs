namespace LocMp.Order.Application.DTOs;

public sealed record OrderItemDto(
    Guid Id,
    Guid ProductId,
    string ProductName,
    string? ProductDescription,
    string? MainPhotoUrl,
    decimal UnitPrice,
    int Quantity,
    decimal Subtotal);
