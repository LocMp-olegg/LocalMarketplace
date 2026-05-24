using LocMp.BuildingBlocks.Application.Exceptions;
using LocMp.Chat.Application.DTOs;
using LocMp.Chat.Application.Mapping;
using LocMp.Chat.Domain.Enums;
using LocMp.Chat.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LocMp.Chat.Application.Chats.Queries.GetChatById;

public sealed class GetChatByIdQueryHandler(ChatDbContext db)
    : IRequestHandler<GetChatByIdQuery, ChatDto>
{
    public async Task<ChatDto> Handle(GetChatByIdQuery request, CancellationToken ct)
    {
        var chat = await db.Chats
                       .Include(c => c.Participants)
                       .FirstOrDefaultAsync(c => c.Id == request.ChatId, ct)
                   ?? throw new NotFoundException("Chat not found.");

        var isParticipant = chat.Participants.Any(p => p.UserId == request.UserId);
        var isAdminAccessingSupport = request.IsAdmin && chat.Type == ChatType.Support;

        if (!isParticipant && !isAdminAccessingSupport && !request.IsAdmin)
            throw new ForbiddenException("Access denied.");

        return ChatMapper.ToChatDto(chat);
    }
}