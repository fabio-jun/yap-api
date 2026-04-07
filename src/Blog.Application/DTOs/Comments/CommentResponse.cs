namespace Blog.Application.DTOs.Comments;

public class CommentResponse
{
    public int Id { get; set; }
    public required string Content { get; set; }
    public DateTime CreatedAt { get; set; }
    public int AuthorId { get; set; }
    public required string AuthorName { get; set; }
}