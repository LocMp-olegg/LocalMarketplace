namespace LocMp.Analytics.Application.DTOs;

public sealed record SellerRatingHistoryDto(
    DateOnly RecordedAt,
    decimal AverageRating,
    int ReviewCount,
    int NewReviewsToday);
