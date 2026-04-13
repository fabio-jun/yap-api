namespace Blog.Domain.Entities;

public class Repost
{
    public int Id { get; set; }
    public string? QuoteContent { get; set; }
    public DateTime CreatedAt { get; set; }

    public int UserId { get; set; }
    public User? User { get; set; }

    public int PostId { get; set; }
    public Post? Post { get; set; }
}
