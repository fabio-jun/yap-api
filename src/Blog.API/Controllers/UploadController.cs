using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Blog.API.Controllers;

// API controller for file uploads to Cloudinary (cloud media storage).
// Handles image and video uploads for posts and profile avatars.
// Files are uploaded to Cloudinary and the secure URL is returned to the client.
[ApiController]
[Route("api/[controller]")]
public class UploadController : ControllerBase
{
    // Cloudinary — SDK client for the Cloudinary media management service.
    // Registered as Singleton in Program.cs (one shared instance, thread-safe).
    private readonly Cloudinary _cloudinary;

    public UploadController(Cloudinary cloudinary) => _cloudinary = cloudinary;

    // POST api/upload — uploads a file (image or video) to Cloudinary.
    // IFormFile — ASP.NET Core abstraction for an uploaded file from a multipart/form-data request.
    // The frontend sends the file via FormData (not JSON).
    [HttpPost]
    [Authorize]
    [SwaggerOperation(Summary = "Upload media", Description = "Uploads an image or video to Cloudinary and returns its secure URL. Images must be under 5MB; videos must be under 50MB.")]
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
        // OpenReadStream() — opens the uploaded file as a readable stream for Cloudinary.
        using var stream = file.OpenReadStream();

        if (isVideo)
        {
            // VideoUploadParams — Cloudinary SDK class for video-specific upload settings.
            // FileDescription — wraps the filename and stream for the SDK.
            // Folder = "yap" — organizes uploads in a "yap" folder on Cloudinary.
            var videoParams = new VideoUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                Folder = "yap"
            };
            var videoResult = await _cloudinary.UploadAsync(videoParams);
            if (videoResult.Error != null)
                return BadRequest(videoResult.Error.Message);
            // SecureUrl — HTTPS URL to the uploaded file on Cloudinary's CDN.
            return Ok(new { url = videoResult.SecureUrl.ToString(), type = "video" });
        }

        // ImageUploadParams — Cloudinary SDK class for image-specific upload settings.
        var imageParams = new ImageUploadParams
        {
            File = new FileDescription(file.FileName, stream),
            Folder = "yap"
        };
        var imageResult = await _cloudinary.UploadAsync(imageParams);
        if (imageResult.Error != null)
            return BadRequest(imageResult.Error.Message);
        return Ok(new { url = imageResult.SecureUrl.ToString(), type = "image" });
    }
}
