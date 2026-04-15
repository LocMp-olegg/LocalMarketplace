using LocMp.Analytics.Infrastructure.BackgroundServices;
using LocMp.Analytics.Infrastructure.Persistence;
using LocMp.BuildingBlocks.Application.Interfaces;
using LocMp.BuildingBlocks.Infrastructure.Events;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LocMp.Analytics.Infrastructure.Extensions;

public static class InfrastructureExtension
{
    public static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("LocalMarketplaceDb")
                               ?? throw new InvalidOperationException(
                                   "Connection string 'LocalMarketplaceDb' not found.");

        services.AddDbContext<AnalyticsDbContext>(options =>
            options.UseNpgsql(connectionString));

        var rabbitMqHost = configuration.GetConnectionString("RabbitMq")
                           ?? "amqp://guest:guest@localhost:5672";

        services.AddMassTransit(x =>
        {
            x.SetKebabCaseEndpointNameFormatter();

            x.AddConsumers(typeof(InfrastructureExtension).Assembly);

            x.UsingRabbitMq((ctx, cfg) =>
            {
                cfg.Host(new Uri(rabbitMqHost));

                cfg.UseMessageRetry(r => { r.Interval(3, TimeSpan.FromSeconds(2)); });

                cfg.ConfigureEndpoints(ctx);
            });
        });

        services.AddScoped<IEventBus, MassTransitEventBus>();

        services.AddHostedService<ViewCountResetBackgroundService>();
        services.AddHostedService<LeaderboardComputeBackgroundService>();

        var redisConnection = configuration.GetConnectionString("Redis")
                              ?? "localhost:6379";
        services.AddStackExchangeRedisCache(opts =>
        {
            opts.Configuration = redisConnection;
            opts.InstanceName = "locmp-analytics:";
        });
    }
}
