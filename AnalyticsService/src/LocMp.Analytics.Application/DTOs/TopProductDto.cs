using LocMp.Analytics.Domain.Enums;

namespace LocMp.Analytics.Application.DTOs;

public sealed record TopProductDto(
    Guid ProductId,
    string ProductName,
    PeriodType PeriodType,
    DateOnly PeriodStart,
    int TotalSold,
    decimal TotalRevenue,
    int ViewCount,
    int FavoriteCount,
    DateTimeOffset UpdatedAt);
