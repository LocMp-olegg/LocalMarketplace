using LocMp.Chat.Application.DTOs;
using LocMp.Chat.Domain.Enums;
using MediatR;

namespace LocMp.Chat.Application.Chats.Commands.CreateChat;

public sealed record CreateChatCommand(
    ChatType Type,
    Guid InitiatorId,
    string InitiatorName,
    Guid? TargetUserId,
    string? TargetUserName,
    Guid? ReferenceId,
    string? InitialMessage
) : IRequest<ChatDto>;