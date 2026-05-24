namespace LocMp.Contracts.Chat;

public sealed record ChatMessageSentEvent(
    Guid ChatId,
    Guid MessageId,
    Guid SenderId,
    string SenderName,
    Guid[] RecipientIds,
    string ChatType,
    DateTimeOffset OccurredAt) : IIntegrationEvent;
