using LocMp.Review.Application.DTOs;
using MediatR;

namespace LocMp.Review.Application.Reviews.Queries.GetReviewById;

public sealed record GetReviewByIdQuery(Guid ReviewId) : IRequest<ReviewDto?>;
