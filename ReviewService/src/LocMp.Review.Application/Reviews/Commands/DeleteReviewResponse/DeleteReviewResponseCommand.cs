using MediatR;

namespace LocMp.Review.Application.Reviews.Commands.DeleteReviewResponse;

public sealed record DeleteReviewResponseCommand(
    Guid ReviewId,
    Guid RequesterId,
    bool IsAdmin) : IRequest;
