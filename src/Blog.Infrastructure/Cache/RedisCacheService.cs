using System.Text.Json;
using Blog.Application.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Blog.Infrastructure.Cache;

public class RedisCacheService : ICacheService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly IDistributedCache _cache;
    private readonly IConnectionMultiplexer _connection;
    private readonly ILogger<RedisCacheService> _logger;

    public RedisCacheService(
        IDistributedCache cache,
        IConnectionMultiplexer connection,
        ILogger<RedisCacheService> logger)
    {
        _cache = cache;
        _connection = connection;
        _logger = logger;
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken ct = default)
    {
        try
        {
            var json = await _cache.GetStringAsync(key, ct);
            return string.IsNullOrWhiteSpace(json)
                ? default
                : JsonSerializer.Deserialize<T>(json, JsonOptions);
        }
        catch (Exception ex) when (IsCacheException(ex))
        {
            _logger.LogWarning(ex, "Failed to read cache key {CacheKey}", key);
            return default;
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan ttl, CancellationToken ct = default)
    {
        if (value is null)
        {
            return;
        }

        try
        {
            var json = JsonSerializer.Serialize(value, JsonOptions);
            await _cache.SetStringAsync(
                key,
                json,
                new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = ttl },
                ct);
        }
        catch (Exception ex) when (IsCacheException(ex))
        {
            _logger.LogWarning(ex, "Failed to write cache key {CacheKey}", key);
        }
    }

    public async Task RemoveAsync(string key, CancellationToken ct = default)
    {
        try
        {
            await _cache.RemoveAsync(key, ct);
        }
        catch (Exception ex) when (IsCacheException(ex))
        {
            _logger.LogWarning(ex, "Failed to remove cache key {CacheKey}", key);
        }
    }

    public async Task RemoveByPrefixAsync(string prefix, CancellationToken ct = default)
    {
        try
        {
            var pattern = $"{prefix}*";
            foreach (var endpoint in _connection.GetEndPoints())
            {
                var server = _connection.GetServer(endpoint);
                if (!server.IsConnected)
                {
                    continue;
                }

                await foreach (var key in server.KeysAsync(pattern: pattern))
                {
                    ct.ThrowIfCancellationRequested();
                    await _cache.RemoveAsync(key.ToString(), ct);
                }
            }
        }
        catch (Exception ex) when (IsCacheException(ex))
        {
            _logger.LogWarning(ex, "Failed to remove cache keys with prefix {CachePrefix}", prefix);
        }
    }

    private static bool IsCacheException(Exception ex)
    {
        return ex is RedisException
            or TimeoutException
            or JsonException
            or InvalidOperationException;
    }
}
