namespace Blog.Application.DTOs.Comments;

// DTO for creating a comment on a post.
// The PostId comes from the route parameter, AuthorId from the JWT token.
public class CreateCommentRequest
{
    public required string Content { get; set; }
}
