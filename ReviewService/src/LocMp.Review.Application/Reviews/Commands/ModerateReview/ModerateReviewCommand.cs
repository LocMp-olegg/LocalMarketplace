using MediatR;

namespace LocMp.Review.Application.Reviews.Commands.ModerateReview;

public sealed record ModerateReviewCommand(Guid ReviewId, bool IsVisible) : IRequest;