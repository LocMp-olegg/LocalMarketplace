using LocMp.Chat.Domain.Entities;
using LocMp.Chat.Domain.Enums;
using LocMp.Chat.Infrastructure.Persistence;
using LocMp.Chat.Infrastructure.Services;
using LocMp.Contracts.Orders;
using MassTransit;
using ChatEntity = LocMp.Chat.Domain.Entities.Chat;

namespace LocMp.Chat.Infrastructure.Consumers;

public sealed class OrderPlacedConsumer(
    ChatDbContext db,
    IChatEncryptionService encryption) : IConsumer<OrderPlacedEvent>
{
    public async Task Consume(ConsumeContext<OrderPlacedEvent> context)
    {
        var msg = context.Message;
        var now = DateTimeOffset.UtcNow;
        var chatId = Guid.NewGuid();
        var encryptionKey = encryption.GenerateChatKey();

        var chat = new ChatEntity(chatId)
        {
            Type = ChatType.Order,
            Status = ChatStatus.Active,
            ReferenceId = msg.OrderId,
            EncryptionKey = encryptionKey,
            CreatedAt = now
        };

        chat.Participants.Add(new ChatParticipant(Guid.NewGuid())
        {
            ChatId = chatId,
            UserId = msg.BuyerId,
            Role = ParticipantRole.Initiator,
            JoinedAt = now
        });

        chat.Participants.Add(new ChatParticipant(Guid.NewGuid())
        {
            ChatId = chatId,
            UserId = msg.SellerId,
            Role = ParticipantRole.Responder,
            JoinedAt = now
        });

        db.Chats.Add(chat);
        await db.SaveChangesAsync(context.CancellationToken);
    }
}