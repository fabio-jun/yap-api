namespace Blog.Application.DTOs.Posts;

// DTO for updating a post
public class UpdatePostRequest
{
    public required string Content { get; set; }
    public string? ImageUrl { get; set; }
}
