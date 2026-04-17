namespace LocMp.Analytics.Application.DTOs;

public sealed record ProductRatingSummaryDto(
    Guid ProductId,
    string ProductName,
    decimal AverageRating,
    int ReviewCount);
