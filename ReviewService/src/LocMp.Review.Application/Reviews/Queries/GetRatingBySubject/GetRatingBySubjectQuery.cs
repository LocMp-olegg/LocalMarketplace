using LocMp.BuildingBlocks.Application.Common;
using LocMp.Review.Application.DTOs;
using LocMp.Review.Domain.Enums;
using MediatR;

namespace LocMp.Review.Application.Reviews.Queries.GetRatingBySubject;

public sealed record GetRatingBySubjectQuery(
    ReviewSubjectType SubjectType,
    Guid SubjectId) : IRequest<RatingAggregateDto>, ICacheableQuery
{
    public string CacheKey => $"rating:{SubjectType}:{SubjectId}";
    public TimeSpan Ttl => TimeSpan.FromMinutes(5);
}