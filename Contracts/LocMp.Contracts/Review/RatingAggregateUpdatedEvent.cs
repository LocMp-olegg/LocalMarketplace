namespace LocMp.Contracts.Review;

public sealed record RatingAggregateUpdatedEvent(
    Guid SubjectId,
    string SubjectType,
    decimal NewAverage,
    int ReviewCount,
    DateTimeOffset OccurredAt,
    Guid? SellerId = null) : IIntegrationEvent;