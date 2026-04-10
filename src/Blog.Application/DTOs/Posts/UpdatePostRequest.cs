namespace Blog.Application.DTOs.Posts;

// DTO for updating an existing post.
// Same fields as CreatePostRequest — the service handles authorization (author or admin only).
public class UpdatePostRequest
{
    public required string Content { get; set; }
    public string? ImageUrl { get; set; }
}
