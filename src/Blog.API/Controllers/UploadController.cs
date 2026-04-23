using Blog.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Blog.API.Controllers;

// API controller for file uploads to Azure Blob Storage.
// Handles image and video uploads for posts and profile avatars.
// Files are uploaded to Azure Blob Storage and the public URL is returned to the client.
[ApiController]
[Route("api/[controller]")]
public class UploadController : ControllerBase
{
    private readonly IFileStorageService _fileStorageService;

    public UploadController(IFileStorageService fileStorageService) => _fileStorageService = fileStorageService;

    // POST api/upload — uploads a file (image or video) to Azure Blob Storage.
    // IFormFile — ASP.NET Core abstraction for an uploaded file from a multipart/form-data request.
    // The frontend sends the file via FormData (not JSON).
    [HttpPost]
    [Authorize]
    [SwaggerOperation(Summary = "Upload media", Description = "Uploads an image or video to Azure Blob Storage and returns its public URL. Images must be under 5MB; videos must be under 50MB.")]
    public async Task<IActionResult> Upload(IFormFile file)
    {
        // Input validation — reject empty or missing files.
        if (file == null || file.Length == 0)
            return BadRequest("No file uploaded.");

        // Whitelist of allowed MIME types for images and videos.
        // new[] { ... } — implicitly typed array (compiler infers string[]).
        var imageTypes = new[] { "image/jpeg", "image/png", "image/gif", "image/webp" };
        var videoTypes = new[] { "video/mp4", "video/webm", "video/quicktime" };
        // Contains() — LINQ extension method that checks if the array includes the value.
        var isImage = imageTypes.Contains(file.ContentType);
        var isVideo = videoTypes.Contains(file.ContentType);

        if (!isImage && !isVideo)
            return BadRequest("Only image and video files are allowed.");

        // File size limits: 5MB for images, 50MB for videos.
        // file.Length is in bytes — multiply constants for readability.
        if (isImage && file.Length > 5 * 1024 * 1024)
            return BadRequest("Image size must be under 5MB.");

        if (isVideo && file.Length > 50 * 1024 * 1024)
            return BadRequest("Video size must be under 50MB.");

        // using var — ensures the stream is disposed after the block (IDisposable pattern).
        // OpenReadStream() — opens the uploaded file as a readable stream for storage upload.
        using var stream = file.OpenReadStream();
        var result = await _fileStorageService.UploadAsync(stream, file.FileName, file.ContentType, HttpContext.RequestAborted);
        return Ok(result);
    }
}
