using MediatR;

namespace LocMp.Order.Application.Orders.Commands.Orders.CancelOrder;

public sealed record CancelOrderCommand(
    Guid OrderId,
    Guid RequesterId,
    bool IsAdmin,
    string? Comment) : IRequest;
