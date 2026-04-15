using LocMp.Contracts.Orders;
using MediatR;

namespace LocMp.Order.Application.Orders.Commands.Disputes.OpenDispute;

public sealed record OpenDisputeCommand(
    Guid OrderId,
    Guid InitiatorId,
    DisputeType DisputeType,
    string Reason) : IRequest;