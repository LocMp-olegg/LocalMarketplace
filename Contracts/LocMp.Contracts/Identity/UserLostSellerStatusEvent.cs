namespace LocMp.Contracts.Identity;

public sealed record UserLostSellerStatusEvent(
    Guid UserId,
    DateTimeOffset OccurredAt) : IIntegrationEvent;
