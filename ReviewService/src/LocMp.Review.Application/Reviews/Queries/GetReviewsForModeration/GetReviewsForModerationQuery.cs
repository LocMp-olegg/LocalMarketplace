using LocMp.BuildingBlocks.Application.Common;
using LocMp.Review.Application.DTOs;
using MediatR;

namespace LocMp.Review.Application.Reviews.Queries.GetReviewsForModeration;

public sealed record GetReviewsForModerationQuery(
    bool? IsVisible = null,
    int Page = 1,
    int PageSize = 20) : IRequest<PagedResult<ReviewSummaryDto>>;
