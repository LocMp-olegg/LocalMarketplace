using MediatR;

namespace LocMp.Order.Application.Orders.Commands.Orders.MarkOrderPickedUp;

public sealed record MarkOrderPickedUpCommand(Guid OrderId, Guid CourierId) : IRequest;
