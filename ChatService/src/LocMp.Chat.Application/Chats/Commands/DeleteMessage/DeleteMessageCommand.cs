using MediatR;

namespace LocMp.Chat.Application.Chats.Commands.DeleteMessage;

public sealed record DeleteMessageCommand(Guid MessageId, Guid UserId, bool IsAdmin) : IRequest;