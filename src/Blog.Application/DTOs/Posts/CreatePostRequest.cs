namespace Blog.Application.DTOs.Posts;

public class CreatePostRequest
{
    // Post text content — max 280 characters (validated in PostService)
    public required string Content { get; set; }

    // Optional media URL from object storage (uploaded separately via /api/upload)
    public string? ImageUrl { get; set; }
}
