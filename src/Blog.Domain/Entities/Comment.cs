namespace Blog.Domain.Entities;

public class Comment
{
    public int Id { get; set; }
    public required string Content { get; set; }
    public DateTime CreatedAt { get; set; }
    public int PostId { get; set; }
    public Post? Post { get; set; } // Navigation property (doesn't turn it into a column)
    public int AuthorId { get; set; }
    public User? Author { get; set; }
}