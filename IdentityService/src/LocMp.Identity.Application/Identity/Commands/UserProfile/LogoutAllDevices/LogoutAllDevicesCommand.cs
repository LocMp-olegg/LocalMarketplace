using MediatR;

namespace LocMp.Identity.Application.Identity.Commands.UserProfile.LogoutAllDevices;

public sealed record LogoutAllDevicesCommand(Guid UserId) : IRequest;
