namespace Blog.Domain.Entities;

// Represents a like on a post
// A user can only like a post once
public class Like
{
    public int PostId { get; set; }
    public Post? Post { get; set; }
    public int UserId { get; set; }
    public User? User { get; set; }
    public DateTime CreatedAt { get; set; }
}
