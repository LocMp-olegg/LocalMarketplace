namespace LocMp.BuildingBlocks.Application.Common;

/// <summary>
/// Маркерный интерфейс для запросов, результат которых кэшируется в Redis.
/// Реализовать в Query-классах, которые обрабатываются CachingBehavior.
/// </summary>
public interface ICacheableQuery
{
    string CacheKey { get; }
    TimeSpan Ttl { get; }
}