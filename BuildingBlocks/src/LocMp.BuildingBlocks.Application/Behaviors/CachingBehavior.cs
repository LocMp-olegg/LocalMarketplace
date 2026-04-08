using System.Text.Json;
using LocMp.BuildingBlocks.Application.Common;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace LocMp.BuildingBlocks.Application.Behaviors;

public sealed class CachingBehavior<TRequest, TResponse>(
    IDistributedCache cache,
    ILogger<CachingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        if (request is not ICacheableQuery cacheableQuery)
            return await next(ct);

        var cached = await cache.GetStringAsync(cacheableQuery.CacheKey, ct);
        if (cached is not null)
        {
            logger.LogDebug("Cache hit for key '{Key}'", cacheableQuery.CacheKey);
            return JsonSerializer.Deserialize<TResponse>(cached)!;
        }

        var response = await next(ct);

        var options = new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = cacheableQuery.Ttl };
        await cache.SetStringAsync(cacheableQuery.CacheKey, JsonSerializer.Serialize(response), options, ct);

        logger.LogDebug("Cache miss for key '{Key}', result stored with TTL {Ttl}", cacheableQuery.CacheKey,
            cacheableQuery.Ttl);

        return response;
    }
}