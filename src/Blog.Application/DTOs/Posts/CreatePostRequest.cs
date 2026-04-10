namespace Blog.Application.DTOs.Posts;

// DTO for creating a new post (yap).
// The AuthorId is NOT in this DTO — it comes from the JWT token (server-side, not client-controllable).
public class CreatePostRequest
{
    // Post text content — max 280 characters (validated in PostService)
    public required string Content { get; set; }

    // Optional media URL from Cloudinary (uploaded separately via /api/upload)
    public string? ImageUrl { get; set; }
}
