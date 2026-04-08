using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Blog.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UploadController : ControllerBase
{
    private readonly Cloudinary _cloudinary;

    public UploadController(Cloudinary cloudinary) => _cloudinary = cloudinary;

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Upload(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("No file uploaded.");

        var imageTypes = new[] { "image/jpeg", "image/png", "image/gif", "image/webp" };
        var videoTypes = new[] { "video/mp4", "video/webm", "video/quicktime" };
        var isImage = imageTypes.Contains(file.ContentType);
        var isVideo = videoTypes.Contains(file.ContentType);

        if (!isImage && !isVideo)
            return BadRequest("Only image and video files are allowed.");

        if (isImage && file.Length > 5 * 1024 * 1024)
            return BadRequest("Image size must be under 5MB.");

        if (isVideo && file.Length > 50 * 1024 * 1024)
            return BadRequest("Video size must be under 50MB.");

        using var stream = file.OpenReadStream();

        if (isVideo)
        {
            var videoParams = new VideoUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                Folder = "yap"
            };
            var videoResult = await _cloudinary.UploadAsync(videoParams);
            if (videoResult.Error != null)
                return BadRequest(videoResult.Error.Message);
            return Ok(new { url = videoResult.SecureUrl.ToString(), type = "video" });
        }

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
