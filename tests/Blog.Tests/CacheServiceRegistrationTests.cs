using Blog.API.DependencyInjection;
using Blog.Application.Interfaces;
using Blog.Infrastructure.Cache;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace Blog.Tests;

public class CacheServiceRegistrationTests
{
    [Fact]
    public void AddConfiguredCaching_WithoutProvider_RegistersRedisServicesUsingDefaultConnection()
    {
        var services = new ServiceCollection();
        var config = new ConfigurationBuilder().Build();

        services.AddLogging();

        services.AddConfiguredCaching(config);

        Assert.Contains(services, descriptor =>
            descriptor.ServiceType == typeof(ICacheService) &&
            descriptor.ImplementationType == typeof(RedisCacheService));
        Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(IConnectionMultiplexer));
    }

    [Fact]
    public void AddConfiguredCaching_WithCustomRedisConnection_RegistersRedisServices()
    {
        var services = new ServiceCollection();
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Redis:Connection"] = "localhost:6380"
            })
            .Build();

        services.AddLogging();

        services.AddConfiguredCaching(config);

        Assert.Contains(services, descriptor =>
            descriptor.ServiceType == typeof(ICacheService) &&
            descriptor.ImplementationType == typeof(RedisCacheService));
        Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(IConnectionMultiplexer));
    }
}
