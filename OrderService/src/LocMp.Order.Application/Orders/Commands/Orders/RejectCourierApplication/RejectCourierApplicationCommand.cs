using MediatR;

namespace LocMp.Order.Application.Orders.Commands.Orders.RejectCourierApplication;

public sealed record RejectCourierApplicationCommand(
    Guid SellerId,
    Guid ApplicationId) : IRequest;