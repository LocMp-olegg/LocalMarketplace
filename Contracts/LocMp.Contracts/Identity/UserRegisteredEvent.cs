namespace LocMp.Contracts.Identity;

public sealed record UserRegisteredEvent(
    Guid UserId,
    string Email,
    string DisplayName,
    DateTimeOffset OccurredAt) : IIntegrationEvent;