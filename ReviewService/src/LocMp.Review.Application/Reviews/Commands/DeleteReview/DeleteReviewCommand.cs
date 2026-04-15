using MediatR;

namespace LocMp.Review.Application.Reviews.Commands.DeleteReview;

public sealed record DeleteReviewCommand(Guid ReviewId, Guid RequesterId, bool IsAdmin = false) : IRequest;