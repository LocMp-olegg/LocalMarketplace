using MediatR;

namespace LocMp.Order.Application.Orders.Commands.Orders.ResolveDispute;

public sealed record ResolveDisputeCommand(
    Guid DisputeId,
    Guid AdminId,
    string Resolution) : IRequest;
