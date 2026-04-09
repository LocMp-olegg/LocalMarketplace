namespace LocMp.Contracts.Identity;

public sealed record ShopProfileUpdatedEvent(
    Guid ShopId,
    Guid UserId,
    string BusinessName,
    string? Description,
    string? WorkingHours,
    int? ServiceRadiusMeters,
    bool IsActive,
    DateTimeOffset OccurredAt) : IIntegrationEvent;