using MediatR;

namespace LocMp.Order.Application.Orders.Commands.Orders.MarkOrderDelivered;

public sealed record MarkOrderDeliveredCommand(Guid OrderId, Guid CourierId) : IRequest;
