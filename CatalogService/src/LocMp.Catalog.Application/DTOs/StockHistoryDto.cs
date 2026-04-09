using LocMp.Catalog.Domain.Enums;

namespace LocMp.Catalog.Application.DTOs;

public sealed record StockHistoryDto(
    Guid Id,
    Guid ProductId,
    StockChangeType ChangeType,
    int QuantityDelta,
    int QuantityAfter,
    Guid? ReferenceId,
    DateTimeOffset CreatedAt
);
