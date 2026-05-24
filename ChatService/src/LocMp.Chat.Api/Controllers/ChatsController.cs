using LocMp.BuildingBlocks.Application.Common;
using LocMp.BuildingBlocks.Infrastructure.Extensions;
using LocMp.Chat.Api.Requests;
using LocMp.Chat.Application.Chats.Commands.CloseChat;
using LocMp.Chat.Application.Chats.Commands.CreateChat;
using LocMp.Chat.Application.Chats.Commands.DeleteMessage;
using LocMp.Chat.Application.Chats.Commands.MarkMessagesRead;
using LocMp.Chat.Application.Chats.Commands.SendMessage;
using LocMp.Chat.Application.Chats.Queries.GetChatById;
using LocMp.Chat.Application.Chats.Queries.GetChatMessages;
using LocMp.Chat.Application.Chats.Queries.GetMyChats;
using LocMp.Chat.Application.Chats.Queries.GetSupportChats;
using LocMp.Chat.Application.Chats.Queries.GetUnreadCount;
using LocMp.Chat.Application.DTOs;
using LocMp.Chat.Application.Enums;
using LocMp.Chat.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LocMp.Chat.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public sealed class ChatsController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResult<ChatSummaryDto>>> GetMyChats(
        [FromQuery] ChatType? type,
        [FromQuery] ChatStatus? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
        => Ok(await sender.Send(new GetMyChatsQuery(HttpContext.GetUserId(), type, status, page, pageSize), ct));

    [HttpGet("support")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<PagedResult<ChatSummaryDto>>> GetSupportChats(
        [FromQuery] Guid? userId,
        [FromQuery] ChatStatus? status,
        [FromQuery] SupportChatSortBy sortBy = SupportChatSortBy.Newest,
        [FromQuery] bool? hasUnread = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
        => Ok(await sender.Send(
            new GetSupportChatsQuery(HttpContext.GetUserId(), userId, status, sortBy, hasUnread, page, pageSize), ct));

    [HttpGet("unread-count")]
    public async Task<ActionResult<int>> GetUnreadCount(CancellationToken ct = default)
        => Ok(await sender.Send(new GetUnreadCountQuery(HttpContext.GetUserId(), HttpContext.IsInRole("Admin")), ct));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ChatDto>> GetById(Guid id, CancellationToken ct = default)
        => Ok(await sender.Send(new GetChatByIdQuery(id, HttpContext.GetUserId(), HttpContext.IsInRole("Admin")), ct));

    [HttpGet("{id:guid}/messages")]
    public async Task<ActionResult<PagedResult<MessageDto>>> GetMessages(
        Guid id,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken ct = default)
        => Ok(await sender.Send(
            new GetChatMessagesQuery(id, HttpContext.GetUserId(), HttpContext.IsInRole("Admin"), page, pageSize), ct));

    [HttpPost]
    public async Task<ActionResult<ChatDto>> Create([FromBody] CreateChatRequest request,
        CancellationToken ct = default)
    {
        var userName = User.FindFirst("username")?.Value ?? string.Empty;

        var result = await sender.Send(new CreateChatCommand(
            request.Type,
            HttpContext.GetUserId(),
            userName,
            request.TargetUserId,
            request.TargetUserName,
            request.ReferenceId,
            request.InitialMessage), ct);

        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPost("{id:guid}/messages")]
    public async Task<ActionResult<MessageDto>> SendMessage(
        Guid id,
        [FromForm] SendMessageRequest request,
        CancellationToken ct = default)
    {
        var userName = User.FindFirst("username")?.Value ?? string.Empty;

        return Ok(await sender.Send(new SendMessageCommand(
            id,
            HttpContext.GetUserId(),
            userName,
            request.Body,
            MessageType.User,
            HttpContext.IsInRole("Admin"),
            request.Attachments?.ToList()), ct));
    }

    [HttpPut("{id:guid}/messages/read")]
    public async Task<IActionResult> MarkRead(Guid id, CancellationToken ct = default)
    {
        await sender.Send(new MarkMessagesReadCommand(id, HttpContext.GetUserId(), HttpContext.IsInRole("Admin")), ct);
        return NoContent();
    }

    [HttpDelete("{id:guid}/messages/{messageId:guid}")]
    public async Task<IActionResult> DeleteMessage(Guid id, Guid messageId, CancellationToken ct = default)
    {
        await sender.Send(new DeleteMessageCommand(messageId, HttpContext.GetUserId(), HttpContext.IsInRole("Admin")),
            ct);
        return NoContent();
    }

    [HttpPut("{id:guid}/close")]
    public async Task<IActionResult> Close(Guid id, CancellationToken ct = default)
    {
        await sender.Send(new CloseChatCommand(id, HttpContext.GetUserId(), HttpContext.IsInRole("Admin")), ct);
        return NoContent();
    }
}