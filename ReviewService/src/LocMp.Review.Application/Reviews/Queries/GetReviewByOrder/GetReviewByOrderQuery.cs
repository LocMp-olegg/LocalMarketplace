using LocMp.Review.Application.DTOs;
using MediatR;

namespace LocMp.Review.Application.Reviews.Queries.GetReviewByOrder;

public sealed record GetReviewByOrderQuery(Guid OrderId) : IRequest<ReviewDto?>;