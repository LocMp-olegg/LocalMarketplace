using System.Text.Json;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;

namespace LocMp.BuildingBlocks.Application.Behaviors;

public class IdempotentBehavior<TRequest, TResponse>(
    IHttpContextAccessor httpContextAccessor,
    IDistributedCache cache)
    : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        var context = httpContextAccessor.HttpContext;
        if (context == null || !context.Request.Headers.TryGetValue("Idempotency-Key", out var keyValues))
            return await next(ct);

        var idempotencyKey = $"{typeof(TRequest).Name}_{keyValues.ToString()}";

        // TODO: вероятно, можно добавить проверку использования, например, в Redis
        var cachedResponse = await cache.GetStringAsync(idempotencyKey, ct);
        if (!string.IsNullOrEmpty(cachedResponse))
            return JsonSerializer.Deserialize<TResponse>(cachedResponse)!;

        var response = await next(ct);

        var options = new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24) };
        await cache.SetStringAsync(idempotencyKey, JsonSerializer.Serialize(response), options, ct);

        return response;
    }
}