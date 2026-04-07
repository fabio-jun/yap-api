namespace Blog.Domain.Entities;

public class Bookmark
{
    public int PostId { get; set; }
    public Post? Post { get; set; }
    public int UserId { get; set; }
    public User? User { get; set; }
    public DateTime CreatedAt { get; set; }
}
