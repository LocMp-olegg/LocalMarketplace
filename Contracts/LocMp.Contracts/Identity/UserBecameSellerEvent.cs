namespace LocMp.Contracts.Identity;

public sealed record UserBecameSellerEvent(
    Guid UserId,
    string DisplayName,
    DateTimeOffset OccurredAt) : IIntegrationEvent;