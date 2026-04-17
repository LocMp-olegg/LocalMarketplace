namespace LocMp.Analytics.Application.DTOs;

public sealed record SellerProductRatingsDto(
    IReadOnlyList<ProductRatingSummaryDto> Products,
    decimal OverallAverageRating,
    int TotalReviewCount);
