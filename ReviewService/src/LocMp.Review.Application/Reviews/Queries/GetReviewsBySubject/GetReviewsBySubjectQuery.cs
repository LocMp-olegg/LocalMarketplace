using LocMp.BuildingBlocks.Application.Common;
using LocMp.Review.Application.DTOs;
using LocMp.Review.Domain.Enums;
using MediatR;

namespace LocMp.Review.Application.Reviews.Queries.GetReviewsBySubject;

public sealed record GetReviewsBySubjectQuery(
    ReviewSubjectType SubjectType,
    Guid SubjectId,
    int Page = 1,
    int PageSize = 20,
    bool IncludeHidden = false) : IRequest<PagedResult<ReviewSummaryDto>>;