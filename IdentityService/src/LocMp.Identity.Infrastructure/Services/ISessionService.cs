namespace LocMp.Identity.Infrastructure.Services;

public interface ISessionService
{
    Task RevokeAllSessionsAsync(Guid userId, CancellationToken ct = default);
}