using MediatR;

namespace LocMp.Order.Application.Orders.Commands.Orders.ApproveCourierApplication;

public sealed record ApproveCourierApplicationCommand(
    Guid SellerId,
    Guid ApplicationId) : IRequest;