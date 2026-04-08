namespace LocMp.Contracts.Dispute;

public sealed record DisputeOpenedEvent(
    Guid DisputeId,
    Guid OrderId,
    Guid InitiatorId,
    DateTimeOffset OccurredAt) : IIntegrationEvent;