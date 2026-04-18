using Duende.IdentityServer.Services;

namespace LocMp.Identity.Infrastructure.Services;

public sealed class SessionService(IPersistedGrantService persistedGrantService) : ISessionService
{
    public Task RevokeAllSessionsAsync(Guid userId, CancellationToken ct = default)
        => persistedGrantService.RemoveAllGrantsAsync(userId.ToString());
}