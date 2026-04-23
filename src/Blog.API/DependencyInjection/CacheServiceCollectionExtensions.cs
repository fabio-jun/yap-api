using Blog.Application.Interfaces;
using Blog.Infrastructure.Cache;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using StackExchange.Redis;

namespace Blog.API.DependencyInjection;

public static class CacheServiceCollectionExtensions
{
    public static IServiceCollection AddConfiguredCaching(this IServiceCollection services, IConfiguration configuration)
    {
        var redisConnection = configuration["Redis:Connection"] ?? "localhost:6379";
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = redisConnection;
        });

        services.AddSingleton<IConnectionMultiplexer>(_ =>
        {
            var options = ConfigurationOptions.Parse(redisConnection);
            options.AbortOnConnectFail = false;
            return ConnectionMultiplexer.Connect(options);
        });

        services.AddScoped<ICacheService, RedisCacheService>();
        return services;
    }
}
