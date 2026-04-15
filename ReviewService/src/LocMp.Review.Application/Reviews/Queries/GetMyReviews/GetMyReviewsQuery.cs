using LocMp.BuildingBlocks.Application.Common;
using LocMp.Review.Application.DTOs;
using MediatR;

namespace LocMp.Review.Application.Reviews.Queries.GetMyReviews;

public sealed record GetMyReviewsQuery(
    Guid ReviewerId,
    int Page = 1,
    int PageSize = 20) : IRequest<PagedResult<ReviewSummaryDto>>;
