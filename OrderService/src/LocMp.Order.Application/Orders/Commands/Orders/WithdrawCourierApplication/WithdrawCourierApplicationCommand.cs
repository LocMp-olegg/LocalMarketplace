using MediatR;

namespace LocMp.Order.Application.Orders.Commands.Orders.WithdrawCourierApplication;

public sealed record WithdrawCourierApplicationCommand(
    Guid CourierId,
    Guid ApplicationId) : IRequest;