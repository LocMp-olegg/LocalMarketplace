using LocMp.BuildingBlocks.Infrastructure.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace LocMp.Chat.Api.Hubs;

[Authorize]
public sealed class ChatHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        if (Context.User?.IsInRole("Admin") == true)
            await Groups.AddToGroupAsync(Context.ConnectionId, "Admins");
        await base.OnConnectedAsync();
    }

    public async Task JoinChat(Guid chatId)
        => await Groups.AddToGroupAsync(Context.ConnectionId, chatId.ToString());

    public async Task LeaveChat(Guid chatId)
        => await Groups.RemoveFromGroupAsync(Context.ConnectionId, chatId.ToString());

    public async Task StartTyping(Guid chatId)
    {
        var userId = Context.User?.GetUserIdString();
        var userName = Context.User?.FindFirst("username")?.Value ?? string.Empty;

        await Clients.OthersInGroup(chatId.ToString())
            .SendAsync("typing", new { chatId, userId, userName, isTyping = true });
    }

    public async Task StopTyping(Guid chatId)
    {
        var userId = Context.User?.GetUserIdString();
        var userName = Context.User?.FindFirst("username")?.Value ?? string.Empty;

        await Clients.OthersInGroup(chatId.ToString())
            .SendAsync("typing", new { chatId, userId, userName, isTyping = false });
    }
}