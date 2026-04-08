namespace LocMp.BuildingBlocks.Application.Interfaces;

/// <summary>
/// Абстракция шины интеграционных событий.
/// В Identity подключается как NullEventBus до настройки RabbitMQ.
/// </summary>
public interface IEventBus
{
    Task PublishAsync<T>(T integrationEvent, CancellationToken ct = default) where T : class;
}
