using Blog.API.DependencyInjection;
using Blog.Application.Interfaces;
using Blog.Infrastructure.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Blog.Tests;

public class StorageServiceRegistrationTests
{
    [Fact]
    public void AddConfiguredStorage_LocalProvider_RegistersLocalFileStorageService()
    {
        var services = new ServiceCollection();
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Storage:Provider"] = "Local"
            })
            .Build();

        services.AddLogging();

        services.AddConfiguredStorage(config);

        Assert.Contains(services, descriptor =>
            descriptor.ServiceType == typeof(IFileStorageService) &&
            descriptor.ImplementationType == typeof(LocalFileStorageService));
    }

    [Fact]
    public void AddConfiguredStorage_AzureBlobProvider_RegistersAzureBlobStorageService()
    {
        var services = new ServiceCollection();
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Storage:Provider"] = "AzureBlob"
            })
            .Build();

        services.AddLogging();

        services.AddConfiguredStorage(config);

        Assert.Contains(services, descriptor =>
            descriptor.ServiceType == typeof(IFileStorageService) &&
            descriptor.ImplementationType == typeof(AzureBlobStorageService));
    }
}
