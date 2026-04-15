using LocMp.Review.Domain.Enums;

namespace LocMp.Review.Application.DTOs;

public sealed record RatingAggregateDto(
    Guid SubjectId,
    ReviewSubjectType SubjectType,
    decimal AverageRating,
    int ReviewCount,
    int OneStar,
    int TwoStar,
    int ThreeStar,
    int FourStar,
    int FiveStar,
    DateTimeOffset LastCalculatedAt);
