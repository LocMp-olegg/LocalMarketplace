namespace LocMp.Order.Application.DTOs;

public sealed record CartItemDto(
    Guid Id,
    Guid ProductId,
    string ProductName,
    decimal Price,
    int Quantity,
    decimal Subtotal);
