using LocMp.BuildingBlocks.Application.Interfaces;
using MassTransit;

namespace LocMp.BuildingBlocks.Infrastructure.Events;

public sealed class MassTransitEventBus(IPublishEndpoint publishEndpoint) : IEventBus
{
    public Task PublishAsync<T>(T integrationEvent, CancellationToken ct = default) where T : class
        => publishEndpoint.Publish(integrationEvent, ct);
}