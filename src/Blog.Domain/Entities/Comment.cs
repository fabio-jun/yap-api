namespace Blog.Domain.Entities;

public class Comment
{
    public int Id { get; set; }
    public required string Content { get; set; }
    public DateTime CreatedAt { get; set; }

    // FK 
    public int PostId { get; set; }
    public Post? Post { get; set; }

    // FK
    public int AuthorId { get; set; }
    public User? Author { get; set; }

    // FK (Reply to a comment)
    public int? ParentCommentId { get; set; }
    public Comment? ParentComment { get; set; }
    public ICollection<Comment>? Replies { get; set; }
}
