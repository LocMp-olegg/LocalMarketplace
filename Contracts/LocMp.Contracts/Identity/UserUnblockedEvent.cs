namespace LocMp.Contracts.Identity;

public sealed record UserUnblockedEvent(
    Guid UserId,
    DateTimeOffset OccurredAt) : IIntegrationEvent;