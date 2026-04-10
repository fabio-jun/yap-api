namespace Blog.Application.DTOs.Comments;

// DTO for returning comment data to the client.
// Flattens Comment + User (author) into a single object.
public class CommentResponse
{
    public int Id { get; set; }
    public required string Content { get; set; }
    public DateTime CreatedAt { get; set; }

    // Flattened from Comment.Author
    public int AuthorId { get; set; }
    public required string AuthorName { get; set; }
}
