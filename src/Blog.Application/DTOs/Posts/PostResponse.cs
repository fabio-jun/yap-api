namespace Blog.Application.DTOs.Posts;

// DTO for returning post data to the client
public class PostResponse
{
    public int Id { get; set; }
    public required string Content { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public required int AuthorId { get; set; }
    public required string AuthorName { get; set; }
    public string? AuthorProfileImageUrl { get; set; }
    public string? ImageUrl { get; set; }
    public int LikeCount { get; set; }
    public bool HasLiked { get; set; }
    public bool HasBookmarked { get; set; }
}
