using MediatR;

namespace LocMp.Order.Application.Orders.Commands.Disputes.DeleteDisputePhoto;

public sealed record DeleteDisputePhotoCommand(Guid PhotoId, Guid RequesterId, bool IsAdmin) : IRequest;