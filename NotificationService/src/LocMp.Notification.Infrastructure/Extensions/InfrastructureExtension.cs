using LocMp.BuildingBlocks.Application.Interfaces;
using LocMp.BuildingBlocks.Infrastructure.Events;
using LocMp.Notification.Infrastructure.Persistence;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LocMp.Notification.Infrastructure.Extensions;

public static class InfrastructureExtension
{
    public static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("LocalMarketplaceDb")
                               ?? throw new InvalidOperationException(
                                   "Connection string 'LocalMarketplaceDb' not found.");

        services.AddDbContext<NotificationDbContext>(options =>
            options.UseNpgsql(connectionString));

        var redisConnection = configuration.GetConnectionString("Redis")
                              ?? throw new InvalidOperationException("Connection string 'Redis' not found.");
        services.AddStackExchangeRedisCache(o => o.Configuration = redisConnection);

        var rabbitMqHost = configuration.GetConnectionString("RabbitMq")
                           ?? "amqp://guest:guest@localhost:5672";

        services.AddMassTransit(x =>
        {
            x.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("notifs", false));

            x.AddConsumers(typeof(InfrastructureExtension).Assembly);

            x.UsingRabbitMq((ctx, cfg) =>
            {
                cfg.Host(new Uri(rabbitMqHost));

                cfg.UseMessageRetry(r => { r.Interval(3, TimeSpan.FromSeconds(2)); });

                cfg.ConfigureEndpoints(ctx);
            });
        });

        services.AddScoped<IEventBus, MassTransitEventBus>();
    }
}
