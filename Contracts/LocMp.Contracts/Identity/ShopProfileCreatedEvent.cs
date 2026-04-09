namespace LocMp.Contracts.Identity;

public sealed record ShopProfileCreatedEvent(
    Guid ShopId,
    Guid UserId,
    string BusinessName,
    string SellerDisplayName,
    DateTimeOffset OccurredAt) : IIntegrationEvent;