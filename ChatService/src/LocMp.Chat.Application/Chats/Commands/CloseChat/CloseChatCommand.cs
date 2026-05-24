using MediatR;

namespace LocMp.Chat.Application.Chats.Commands.CloseChat;

public sealed record CloseChatCommand(Guid ChatId, Guid UserId, bool IsAdmin) : IRequest;