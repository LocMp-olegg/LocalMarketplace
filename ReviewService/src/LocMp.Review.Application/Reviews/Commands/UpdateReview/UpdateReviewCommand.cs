using LocMp.Review.Application.DTOs;
using MediatR;

namespace LocMp.Review.Application.Reviews.Commands.UpdateReview;

public sealed record UpdateReviewCommand(
    Guid ReviewId,
    Guid RequesterId,
    int Rating,
    string? Comment) : IRequest<ReviewDto>;