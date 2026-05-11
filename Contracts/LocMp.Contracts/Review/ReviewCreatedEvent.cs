namespace LocMp.Contracts.Review;

public sealed record ReviewCreatedEvent(
    Guid ReviewId,
    Guid SubjectId,
    string SubjectType,
    Guid ReviewerId,
    Guid SellerId,
    int Rating,
    DateTimeOffset OccurredAt,
    string? SubjectName = null) : IIntegrationEvent;
