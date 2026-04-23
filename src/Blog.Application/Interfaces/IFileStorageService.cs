using Blog.Application.DTOs.Uploads;

namespace Blog.Application.Interfaces;

public interface IFileStorageService
{
    Task<UploadFileResponse> UploadAsync(
        Stream stream,
        string fileName,
        string contentType,
        CancellationToken ct = default);
}
