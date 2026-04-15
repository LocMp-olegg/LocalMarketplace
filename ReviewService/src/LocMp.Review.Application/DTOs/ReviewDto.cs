using LocMp.Review.Domain.Enums;

namespace LocMp.Review.Application.DTOs;

public sealed record ReviewDto(
    Guid Id,
    Guid OrderId,
    Guid ReviewerId,
    string ReviewerName,
    ReviewSubjectType SubjectType,
    Guid SubjectId,
    int Rating,
    string? Comment,
    bool IsVisible,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt,
    IReadOnlyList<ReviewPhotoDto> Photos,
    ReviewResponseDto? Response);
