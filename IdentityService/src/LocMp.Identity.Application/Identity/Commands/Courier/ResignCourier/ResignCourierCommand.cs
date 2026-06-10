using MediatR;

namespace LocMp.Identity.Application.Identity.Commands.Courier.ResignCourier;

public sealed record ResignCourierCommand(Guid UserId) : IRequest<Unit>;