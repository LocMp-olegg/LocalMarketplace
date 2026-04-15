using LocMp.BuildingBlocks.Application.Common;
using LocMp.Review.Application.DTOs;
using MediatR;

namespace LocMp.Review.Application.Reviews.Queries.GetAllowedReviewsForBuyer;

public sealed record GetAllowedReviewsForBuyerQuery(
    Guid BuyerId,
    int Page = 1,
    int PageSize = 20) : IRequest<PagedResult<PendingReviewSubjectDto>>;
