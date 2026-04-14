using LocMp.Review.Application.DTOs;
using MediatR;

namespace LocMp.Review.Application.Reviews.Commands.RespondToReview;

public sealed record RespondToReviewCommand(
    Guid ReviewId,
    Guid AuthorId,
    string Comment) : IRequest<ReviewResponseDto>;