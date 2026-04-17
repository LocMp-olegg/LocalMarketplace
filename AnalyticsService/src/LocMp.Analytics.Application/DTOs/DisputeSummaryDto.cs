namespace LocMp.Analytics.Application.DTOs;

public sealed record DisputeSummaryDto(
    DateOnly Date,
    int OpenedCount,
    int ResolvedCount,
    int BuyerFavoredCount,
    int SellerFavoredCount,
    decimal AverageResolutionMinutes,
    DateTimeOffset UpdatedAt);
