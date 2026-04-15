using LocMp.Review.Domain.Enums;

namespace LocMp.Review.Application.DTOs;

/// <summary>
/// Субъект, который покупатель ещё не оценил по завершённому заказу.
/// </summary>
public sealed record PendingReviewSubjectDto(
    Guid OrderId,
    ReviewSubjectType SubjectType,
    Guid SubjectId,
    DateTimeOffset AllowedAt);
