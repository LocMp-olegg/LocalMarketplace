using LocMp.Review.Application.DTOs;
using MediatR;

namespace LocMp.Review.Application.Reviews.Commands.UpdateReviewResponse;

public sealed record UpdateReviewResponseCommand(
    Guid ReviewId,
    Guid AuthorId,
    string Comment) : IRequest<ReviewResponseDto>;
