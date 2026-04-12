using MediatR;

namespace LocMp.Order.Application.Orders.Commands.Photos.DeleteDisputePhoto;

public sealed record DeleteDisputePhotoCommand(Guid PhotoId, Guid RequesterId, bool IsAdmin) : IRequest;
