using LocMp.BuildingBlocks.Application.Interfaces;

namespace LocMp.BuildingBlocks.Infrastructure.Events;

/// <summary>
/// Заглушка до подключения RabbitMQ / MassTransit.
/// Заменяется на MassTransitEventBus при регистрации MassTransit.
/// </summary>
public sealed class NullEventBus : IEventBus
{
    public Task PublishAsync<T>(T integrationEvent, CancellationToken ct = default) where T : class
        => Task.CompletedTask;
}
