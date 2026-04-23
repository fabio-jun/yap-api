using Blog.Application.DTOs.Uploads;
using Blog.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Blog.Infrastructure.Storage;

public class LocalFileStorageService : IFileStorageService
{
    private readonly IHostEnvironment _environment;
    private readonly IConfiguration _configuration;

    public LocalFileStorageService(IHostEnvironment environment, IConfiguration configuration)
    {
        _environment = environment;
        _configuration = configuration;
    }

    public async Task<UploadFileResponse> UploadAsync(
        Stream stream,
        string fileName,
        string contentType,
        CancellationToken ct = default)
    {
        var extension = Path.GetExtension(fileName);
        var type = contentType.StartsWith("video/", StringComparison.OrdinalIgnoreCase) ? "video" : "image";
        var relativeDirectory = Path.Combine("uploads", $"{type}s", DateTime.UtcNow.ToString("yyyy"), DateTime.UtcNow.ToString("MM"));
        var absoluteDirectory = Path.Combine(_environment.ContentRootPath, "wwwroot", relativeDirectory);

        Directory.CreateDirectory(absoluteDirectory);

        var storedFileName = $"{Guid.NewGuid():N}{extension}";
        var absolutePath = Path.Combine(absoluteDirectory, storedFileName);

        await using var destination = File.Create(absolutePath);
        await stream.CopyToAsync(destination, ct);

        var publicPath = $"/{relativeDirectory.Replace('\\', '/')}/{storedFileName}";
        var baseUrl = (_configuration["LocalStorage:BaseUrl"] ?? "http://localhost:8080").TrimEnd('/');

        return new UploadFileResponse
        {
            Url = $"{baseUrl}{publicPath}",
            Type = type
        };
    }
}
