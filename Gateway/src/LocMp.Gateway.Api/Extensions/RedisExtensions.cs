namespace LocMp.Gateway.Api.Extensions;

public static class RedisExtensions
{
    public static void AddRedisBlocklist(this IServiceCollection services, IConfiguration configuration)
    {
        var redisConnection = configuration.GetConnectionString("Redis")
                              ?? throw new InvalidOperationException("Connection string 'Redis' not found.");
        services.AddStackExchangeRedisCache(o => o.Configuration = redisConnection);
    }
}
