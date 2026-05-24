using LocMp.Chat.Application.DTOs;
using LocMp.Chat.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace LocMp.Chat.Application.Chats.Commands.SendMessage;

public sealed record SendMessageCommand(
    Guid ChatId,
    Guid SenderId,
    string SenderName,
    string? Body,
    MessageType MessageType,
    bool IsAdmin,
    IReadOnlyList<IFormFile>? Attachments
) : IRequest<MessageDto>;