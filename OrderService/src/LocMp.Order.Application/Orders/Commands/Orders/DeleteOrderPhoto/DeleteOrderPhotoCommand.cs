using MediatR;

namespace LocMp.Order.Application.Orders.Commands.Orders.DeleteOrderPhoto;

public sealed record DeleteOrderPhotoCommand(Guid PhotoId, Guid RequesterId, bool IsAdmin) : IRequest;