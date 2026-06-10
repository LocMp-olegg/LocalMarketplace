namespace LocMp.Contracts.Identity;

public sealed record UserBecameCourierEvent(
    Guid UserId,
    string DisplayName,
    DateTimeOffset OccurredAt) : IIntegrationEvent;