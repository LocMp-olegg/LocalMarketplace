using MediatR;

namespace LocMp.Order.Application.Orders.Commands.Orders.MarkReadyForCourier;

public sealed record MarkReadyForCourierCommand(Guid OrderId, Guid SellerId) : IRequest;