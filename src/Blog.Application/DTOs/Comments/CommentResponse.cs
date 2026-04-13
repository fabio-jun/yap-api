using Blog.Application.DTOs.Mentions;

namespace Blog.Application.DTOs.Comments;

public class CommentResponse
{
    public int Id { get; set; }
    public required string Content { get; set; }
    public DateTime CreatedAt { get; set; }
    public int AuthorId { get; set; }
    public required string AuthorName { get; set; }
    public int? ParentCommentId { get; set; }
    public List<CommentResponse> Replies { get; set; } = [];
    public List<MentionedUserResponse> MentionedUsers { get; set; } = [];
}
