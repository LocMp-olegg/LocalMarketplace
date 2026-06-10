namespace LocMp.Contracts.Identity;

public sealed record UserLostCourierStatusEvent(
    Guid UserId,
    DateTimeOffset OccurredAt) : IIntegrationEvent;