using MediatR;

namespace LocMp.Order.Application.Orders.Commands.Orders.MarkSellerDelivered;

public sealed record MarkSellerDeliveredCommand(Guid OrderId, Guid SellerId) : IRequest;