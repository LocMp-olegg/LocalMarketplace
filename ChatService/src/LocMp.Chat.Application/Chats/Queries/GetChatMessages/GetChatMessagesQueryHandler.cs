using LocMp.BuildingBlocks.Application.Common;
using LocMp.BuildingBlocks.Application.Exceptions;
using LocMp.BuildingBlocks.Application.Interfaces;
using LocMp.Chat.Application.DTOs;
using LocMp.Chat.Infrastructure.Services;
using LocMp.Chat.Application.Mapping;
using LocMp.Chat.Domain.Enums;
using LocMp.Chat.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LocMp.Chat.Application.Chats.Queries.GetChatMessages;

public sealed class GetChatMessagesQueryHandler(
    ChatDbContext db,
    IChatEncryptionService encryption,
    IStorageService storage)
    : IRequestHandler<GetChatMessagesQuery, PagedResult<MessageDto>>
{
    public async Task<PagedResult<MessageDto>> Handle(GetChatMessagesQuery request, CancellationToken ct)
    {
        var chat = await db.Chats
            .Include(c => c.Participants)
            .FirstOrDefaultAsync(c => c.Id == request.ChatId, ct)
            ?? throw new NotFoundException("Chat not found.");

        var isParticipant = chat.Participants.Any(p => p.UserId == request.UserId);
        var isAdminAccess = request.IsAdmin && chat.Type == ChatType.Support;

        if (!isParticipant && !isAdminAccess && !request.IsAdmin)
            throw new ForbiddenException("Access denied.");

        var total = await db.Messages.CountAsync(m => m.ChatId == request.ChatId, ct);

        var messages = await db.Messages
            .Include(m => m.Attachments)
            .Where(m => m.ChatId == request.ChatId)
            .OrderByDescending(m => m.SentAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(ct);

        var items = messages
            .Select(m => ChatMapper.ToDto(m, encryption, chat.EncryptionKey, storage))
            .ToList();

        return PagedResult<MessageDto>.Create(items, total, request.Page, request.PageSize);
    }
}
