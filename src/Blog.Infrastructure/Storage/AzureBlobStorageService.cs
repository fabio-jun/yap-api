using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Blog.Application.DTOs.Uploads;
using Blog.Application.Interfaces;
using Microsoft.Extensions.Configuration;

namespace Blog.Infrastructure.Storage;

public class AzureBlobStorageService : IFileStorageService
{
    private readonly IConfiguration _configuration;

    public AzureBlobStorageService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<UploadFileResponse> UploadAsync(
        Stream stream,
        string fileName,
        string contentType,
        CancellationToken ct = default)
    {
        var connectionString = _configuration["AzureBlob:ConnectionString"];
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("Azure Blob Storage is not configured. Set AzureBlob:ConnectionString.");
        }

        var containerName = _configuration["AzureBlob:ContainerName"] ?? "yap-media";
        var containerClient = new BlobContainerClient(connectionString, containerName);
        await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob, cancellationToken: ct);

        var extension = Path.GetExtension(fileName);
        var type = contentType.StartsWith("video/", StringComparison.OrdinalIgnoreCase) ? "video" : "image";
        var blobName = $"{type}s/{DateTime.UtcNow:yyyy/MM}/{Guid.NewGuid():N}{extension}";
        var blobClient = containerClient.GetBlobClient(blobName);

        await blobClient.UploadAsync(
            stream,
            new BlobUploadOptions
            {
                HttpHeaders = new BlobHttpHeaders
                {
                    ContentType = contentType
                }
            },
            ct);

        return new UploadFileResponse
        {
            Url = blobClient.Uri.ToString(),
            Type = type
        };
    }
}
