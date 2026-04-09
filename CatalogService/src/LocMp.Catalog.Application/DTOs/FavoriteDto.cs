namespace LocMp.Catalog.Application.DTOs;

public sealed record FavoriteDto(
    Guid Id,
    Guid UserId,
    Guid ProductId,
    string ProductName,
    decimal Price,
    string? MainPhotoUrl,
    DateTimeOffset CreatedAt
);
