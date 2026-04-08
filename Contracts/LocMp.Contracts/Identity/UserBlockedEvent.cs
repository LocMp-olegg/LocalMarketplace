namespace LocMp.Contracts.Identity;

public sealed record UserBlockedEvent(
    Guid UserId,
    DateTimeOffset BlockedUntil,
    DateTimeOffset OccurredAt) : IIntegrationEvent;