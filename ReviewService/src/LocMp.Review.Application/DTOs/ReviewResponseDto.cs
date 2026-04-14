namespace LocMp.Review.Application.DTOs;

public sealed record ReviewResponseDto(
    Guid Id,
    Guid AuthorId,
    string Comment,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);
