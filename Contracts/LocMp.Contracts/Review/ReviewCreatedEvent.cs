namespace LocMp.Contracts.Review;

public sealed record ReviewCreatedEvent(
    Guid ReviewId,
    Guid SubjectId,
    string SubjectType,
    Guid ReviewerId,
    int Rating,
    DateTimeOffset OccurredAt) : IIntegrationEvent;
