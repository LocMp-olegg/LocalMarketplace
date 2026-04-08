namespace LocMp.Contracts.Identity;

public sealed record UserProfileUpdatedEvent(
    Guid UserId,
    string DisplayName,
    string? AvatarUrl,
    DateTimeOffset OccurredAt) : IIntegrationEvent;