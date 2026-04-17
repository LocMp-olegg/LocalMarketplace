namespace LocMp.Analytics.Application.DTOs;

public sealed record ShopRatingsDto(
    Guid? ShopId,
    string? ShopName,
    decimal ShopAverageRating,
    int ShopReviewCount,
    IReadOnlyList<ProductRatingSummaryDto> Products);

public sealed record SellerProductRatingsDto(
    decimal OverallAverageRating,
    int TotalReviewCount,
    IReadOnlyList<ShopRatingsDto> Shops);
