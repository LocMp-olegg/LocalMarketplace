using LocMp.Order.Domain.Enums;
using MediatR;

namespace LocMp.Order.Application.Orders.Commands.Disputes.ResolveDispute;

public sealed record ResolveDisputeCommand(
    Guid DisputeId,
    Guid AdminId,
    DisputeOutcome Outcome,
    string Resolution) : IRequest;