namespace Blog.Domain.Entities;

public class Post
{
    public int Id { get; set; }
    public required string Content { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public User? Author { get; set; }
    public int AuthorId { get; set; }
    public ICollection<Comment>? Comments { get; set; }
    public ICollection<PostTag>? PostTags { get; set; }
    public ICollection<Like>? Likes { get; set; }
    public ICollection<Bookmark>? Bookmarks { get; set; }
    public string? ImageUrl { get; set; }
}
