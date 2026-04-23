namespace Blog.Domain.Entities;

// Short post (max 280 chars), similar to a tweet.
public class Post
{
    public int Id { get; set; }
    public required string Content { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Navigation property — EF Core loads the related User object via .Include(p => p.Author)
    // Nullable because EF Core may not always load it
    public User? Author { get; set; }

    // Foreign Key — references Users.Id. EF Core uses this to build the JOIN.
    public int AuthorId { get; set; }

    // Navigation collections 
    public ICollection<Comment>? Comments { get; set; }
    public ICollection<Like>? Likes { get; set; }
    public ICollection<Bookmark>? Bookmarks { get; set; }
    
    // Many-to-many with Tag through the join entity PostTag
    public ICollection<PostTag>? PostTags { get; set; }
 
    // URL of the image/video uploaded to external object storage.
    public string? ImageUrl { get; set; }
}
