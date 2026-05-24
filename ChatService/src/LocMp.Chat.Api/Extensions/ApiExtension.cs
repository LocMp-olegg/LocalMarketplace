using System.Text.Json.Serialization;
using LocMp.BuildingBlocks.Infrastructure.Middleware;
using LocMp.Chat.Api.Hubs;
using LocMp.Chat.Application.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace LocMp.Chat.Api.Extensions;

public static class ApiExtension
{
    public static void AddApi(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddControllers().AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        });

        var redisConnection = configuration.GetConnectionString("Redis") ?? "localhost:6379";

        services.AddSignalR()
            .AddStackExchangeRedis(redisConnection,
                opts => { opts.Configuration.ChannelPrefix = StackExchange.Redis.RedisChannel.Literal("locmp-chat"); });

        services.AddSingleton<IUserIdProvider, SubClaimUserIdProvider>();
        services.AddScoped<IChatNotifier, SignalRChatNotifier>();

        services.AddHttpContextAccessor();
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();
    }
}