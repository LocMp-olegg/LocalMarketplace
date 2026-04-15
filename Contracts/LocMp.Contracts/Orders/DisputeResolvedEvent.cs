namespace LocMp.Contracts.Orders;

public sealed record DisputeResolvedEvent(
    Guid DisputeId,
    Guid OrderId,
    DisputeType DisputeType,
    DisputeOutcome Outcome,
    Guid BuyerId,
    Guid SellerId,
    Guid? CourierId,
    IReadOnlyList<Guid> ProductIds,
    int ResolutionMinutes,
    DateTimeOffset OccurredAt) : IIntegrationEvent;