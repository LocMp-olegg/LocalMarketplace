using LocMp.Chat.Application.DTOs;
using LocMp.Chat.Application.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace LocMp.Chat.Api.Hubs;

public sealed class SignalRChatNotifier(IHubContext<ChatHub> hub) : IChatNotifier
{
    public async Task NotifyMessageSentAsync(Guid chatId, MessageDto message, IEnumerable<Guid> recipientUserIds,
        bool isSupport = false, CancellationToken ct = default)
    {
        await hub.Clients.Group(chatId.ToString()).SendAsync("message_received", message, ct);

        foreach (var userId in recipientUserIds)
            await hub.Clients.User(userId.ToString()).SendAsync("message_received", message, ct);

        if (isSupport)
            await hub.Clients.Group("Admins").SendAsync("message_received", message, ct);
    }

    public Task NotifyMessageDeletedAsync(Guid chatId, Guid messageId, CancellationToken ct = default)
        => hub.Clients.Group(chatId.ToString())
            .SendAsync("message_deleted", new { chatId, messageId }, ct);

    public Task NotifyChatClosedAsync(Guid chatId, CancellationToken ct = default)
        => hub.Clients.Group(chatId.ToString())
            .SendAsync("chat_closed", new { chatId }, ct);

    public Task NotifyTypingAsync(Guid chatId, Guid userId, string userName, bool isTyping,
        CancellationToken ct = default)
        => hub.Clients.Group(chatId.ToString())
            .SendAsync("typing", new { chatId, userId, userName, isTyping }, ct);

    public Task NotifyMessagesReadAsync(Guid chatId, Guid byUserId, CancellationToken ct = default)
        => hub.Clients.Group(chatId.ToString())
            .SendAsync("messages_read", new { chatId, byUserId }, ct);
}