using MediatR;

namespace LocMp.Order.Application.Orders.Commands.Orders.MarkReadyForPickup;

public sealed record MarkReadyForPickupCommand(Guid OrderId, Guid SellerId) : IRequest;
