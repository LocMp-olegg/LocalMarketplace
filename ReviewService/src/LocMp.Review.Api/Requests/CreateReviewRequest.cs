using LocMp.Review.Domain.Enums;

namespace LocMp.Review.Api.Requests;

public sealed record CreateReviewRequest(
    Guid OrderId,
    ReviewSubjectType SubjectType,
    Guid SubjectId,
    int Rating,
    string? Comment);
