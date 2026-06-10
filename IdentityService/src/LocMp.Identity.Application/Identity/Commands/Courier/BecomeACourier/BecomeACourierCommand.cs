using LocMp.Identity.Application.DTOs.Courier;
using MediatR;

namespace LocMp.Identity.Application.Identity.Commands.Courier.BecomeACourier;

public sealed record BecomeACourierCommand(Guid UserId) : IRequest<CourierProfileDto>;