using LocMp.Identity.Infrastructure.Services;
using MediatR;

namespace LocMp.Identity.Application.Identity.Commands.UserProfile.LogoutAllDevices;

public sealed class LogoutAllDevicesCommandHandler(ISessionService sessionService)
    : IRequestHandler<LogoutAllDevicesCommand>
{
    public Task Handle(LogoutAllDevicesCommand request, CancellationToken ct)
        => sessionService.RevokeAllSessionsAsync(request.UserId, ct);
}
