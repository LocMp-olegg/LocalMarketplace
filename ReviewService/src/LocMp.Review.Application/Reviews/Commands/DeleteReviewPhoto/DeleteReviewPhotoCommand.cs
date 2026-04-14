using MediatR;

namespace LocMp.Review.Application.Reviews.Commands.DeleteReviewPhoto;

public sealed record DeleteReviewPhotoCommand(Guid PhotoId, Guid RequesterId, bool IsAdmin = false) : IRequest;