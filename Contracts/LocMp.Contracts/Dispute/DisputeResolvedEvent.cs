namespace LocMp.Contracts.Dispute;

public sealed record DisputeResolvedEvent(
    Guid DisputeId,
    Guid OrderId,
    int ResolutionMinutes,
    DateTimeOffset OccurredAt) : IIntegrationEvent;