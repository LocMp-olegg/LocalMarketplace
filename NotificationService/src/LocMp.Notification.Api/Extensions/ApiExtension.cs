using System.Text.Json.Serialization;
using LocMp.BuildingBlocks.Infrastructure.Middleware;
using LocMp.Notification.Api.Hubs;
using LocMp.Notification.Domain;
using Microsoft.AspNetCore.SignalR;

namespace LocMp.Notification.Api.Extensions;

public static class ApiExtension
{
    public static void AddApi(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddControllers().AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        });

        services.AddSignalR();
        services.AddSingleton<IUserIdProvider, SubClaimUserIdProvider>();
        services.AddSingleton<INotificationPusher, SignalRNotificationPusher>();

        var allowedOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
        services.AddCors(o => o.AddPolicy("frontend", p =>
            p.WithOrigins(allowedOrigins)
             .AllowAnyHeader()
             .AllowAnyMethod()
             .AllowCredentials()));

        services.AddHttpContextAccessor();
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();
    }
}
