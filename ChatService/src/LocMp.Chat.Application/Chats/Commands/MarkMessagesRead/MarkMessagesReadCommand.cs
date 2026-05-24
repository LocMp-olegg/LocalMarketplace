using MediatR;

namespace LocMp.Chat.Application.Chats.Commands.MarkMessagesRead;

public sealed record MarkMessagesReadCommand(Guid ChatId, Guid UserId, bool IsAdmin = false) : IRequest;