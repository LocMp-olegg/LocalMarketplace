using MediatR;

namespace LocMp.Order.Application.Orders.Commands.Photos.DeleteOrderPhoto;

public sealed record DeleteOrderPhotoCommand(Guid PhotoId, Guid RequesterId, bool IsAdmin) : IRequest;
