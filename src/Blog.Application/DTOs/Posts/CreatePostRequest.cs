namespace Blog.Application.DTOs.Posts;

// DTO for creating a new post
public class CreatePostRequest
{
    public required string Content { get; set; }
    public string? ImageUrl { get; set; }
}
