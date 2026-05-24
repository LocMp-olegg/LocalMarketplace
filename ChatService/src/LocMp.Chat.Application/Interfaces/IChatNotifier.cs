using LocMp.Chat.Application.DTOs;

namespace LocMp.Chat.Application.Interfaces;

public interface IChatNotifier
{
    Task NotifyMessageSentAsync(Guid chatId, MessageDto message, IEnumerable<Guid> recipientUserIds,
        bool isSupport = false, CancellationToken ct = default);

    Task NotifyMessageDeletedAsync(Guid chatId, Guid messageId, CancellationToken ct = default);

    Task NotifyChatClosedAsync(Guid chatId, CancellationToken ct = default);

    Task NotifyTypingAsync(Guid chatId, Guid userId, string userName, bool isTyping, CancellationToken ct = default);

    Task NotifyMessagesReadAsync(Guid chatId, Guid byUserId, CancellationToken ct = default);
}