using Blog.Application.Interfaces;
using Blog.Infrastructure.Storage;

namespace Blog.API.DependencyInjection;

public static class StorageServiceCollectionExtensions
{
    public static IServiceCollection AddConfiguredStorage(this IServiceCollection services, IConfiguration configuration)
    {
        var provider = configuration["Storage:Provider"] ?? "AzureBlob";

        if (provider.Equals("Local", StringComparison.OrdinalIgnoreCase))
        {
            services.AddScoped<IFileStorageService, LocalFileStorageService>();
            return services;
        }

        if (provider.Equals("AzureBlob", StringComparison.OrdinalIgnoreCase))
        {
            services.AddScoped<IFileStorageService, AzureBlobStorageService>();
            return services;
        }

        throw new InvalidOperationException(
            $"Unsupported storage provider '{provider}'. Supported providers: Local, AzureBlob.");
    }
}
