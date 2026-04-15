using LocMp.Review.Domain.Enums;

namespace LocMp.Review.Application.DTOs;

public sealed record ReviewSummaryDto(
    Guid Id,
    Guid ReviewerId,
    string ReviewerName,
    ReviewSubjectType SubjectType,
    Guid SubjectId,
    int Rating,
    string? Comment,
    bool IsVisible,
    DateTimeOffset CreatedAt,
    IReadOnlyList<ReviewPhotoDto> Photos,
    ReviewResponseDto? Response);
