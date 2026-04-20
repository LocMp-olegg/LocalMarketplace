using LocMp.BuildingBlocks.Application.Common;
using LocMp.BuildingBlocks.Infrastructure.Extensions;
using LocMp.Notification.Api.Requests;
using LocMp.Notification.Application.DTOs;
using LocMp.Notification.Application.Notifications.Commands.MarkAllNotificationsRead;
using LocMp.Notification.Application.Notifications.Commands.MarkNotificationRead;
using LocMp.Notification.Application.Notifications.Commands.UpdatePreferences;
using LocMp.Notification.Application.Notifications.Queries.GetNotifications;
using LocMp.Notification.Application.Notifications.Queries.GetPreferences;
using LocMp.Notification.Application.Notifications.Queries.GetUnreadCount;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LocMp.Notification.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public sealed class NotificationsController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResult<NotificationDto>>> GetNotifications(
        [FromQuery] bool? onlyUnread,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
        => Ok(await sender.Send(new GetNotificationsQuery(HttpContext.GetUserId(), onlyUnread, page, pageSize), ct));

    [HttpPost("{id:guid}/read")]
    public async Task<IActionResult> MarkRead(Guid id, CancellationToken ct)
    {
        await sender.Send(new MarkNotificationReadCommand(id, HttpContext.GetUserId()), ct);
        return NoContent();
    }

    [HttpPost("read-all")]
    public async Task<IActionResult> MarkAllRead(CancellationToken ct)
    {
        await sender.Send(new MarkAllNotificationsReadCommand(HttpContext.GetUserId()), ct);
        return NoContent();
    }

    [HttpGet("unread-count")]
    public async Task<ActionResult<int>> GetUnreadCount(CancellationToken ct)
        => Ok(await sender.Send(new GetUnreadCountQuery(HttpContext.GetUserId()), ct));

    [HttpGet("preferences")]
    public async Task<ActionResult<NotificationPreferenceDto>> GetPreferences(CancellationToken ct)
        => Ok(await sender.Send(new GetPreferencesQuery(HttpContext.GetUserId()), ct));

    [HttpPut("preferences")]
    public async Task<ActionResult<NotificationPreferenceDto>> UpdatePreferences(
        [FromBody] UpdatePreferencesRequest request,
        CancellationToken ct)
        => Ok(await sender.Send(
            new UpdatePreferencesCommand(HttpContext.GetUserId(), request.OrderUpdates, request.ReviewReplies,
                request.SystemAlerts, request.EmailEnabled, request.EmailOrderUpdates, request.EmailReviewReplies),
            ct));
}