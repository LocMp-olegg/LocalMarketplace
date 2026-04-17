using LocMp.Analytics.Domain.Enums;

namespace LocMp.Analytics.Application.DTOs;

public sealed record ShopLeaderboardDto(
    Guid ShopId,
    string ShopName,
    decimal TotalRevenue,
    int OrderCount);

public sealed record SellerLeaderboardDto(
    Guid SellerId,
    string SellerName,
    PeriodType PeriodType,
    DateOnly PeriodStart,
    decimal TotalRevenue,
    int OrderCount,
    decimal AverageRating,
    int RevenueRank,
    int OrderCountRank,
    DateTimeOffset? RanksComputedAt,
    IReadOnlyList<ShopLeaderboardDto> Shops);
