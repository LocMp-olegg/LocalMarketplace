using LocMp.BuildingBlocks.Infrastructure.Extensions;
using Microsoft.AspNetCore.SignalR;

namespace LocMp.Notification.Api.Hubs;

public sealed class SubClaimUserIdProvider : IUserIdProvider
{
    public string? GetUserId(HubConnectionContext connection)
        => connection.User.GetUserIdString();
}
