using MediatR;

namespace LocMp.Order.Application.Orders.Commands.Orders.AssignCourier;

public sealed record AssignCourierCommand(
    Guid OrderId,
    Guid CourierId,
    string CourierName,
    string CourierPhone) : IRequest;
