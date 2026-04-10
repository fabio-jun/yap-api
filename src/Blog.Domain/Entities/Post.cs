namespace Blog.Domain.Entities;

// Entity class — represents the "Posts" table in the database.
// A "yap" is a short post (max 280 chars), similar to a tweet.
public class Post
{
    // Primary Key — auto-incremented by the database
    public int Id { get; set; }

    // 'required' ensures Content is always provided when creating a Post object
    public required string Content { get; set; }

    // Timestamp of creation — set to DateTime.UtcNow in the service layer
    public DateTime CreatedAt { get; set; }

    // 'DateTime?' — nullable. Only set when the post is edited.
    public DateTime? UpdatedAt { get; set; }

    // Navigation property — EF Core loads the related User object via .Include(p => p.Author)
    // Nullable because EF Core may not always load it (only when explicitly included)
    public User? Author { get; set; }

    // Foreign Key — references Users.Id. EF Core uses this to build the JOIN.
    public int AuthorId { get; set; }

    // Navigation collections — represent the "many" side of one-to-many relationships
    public ICollection<Comment>? Comments { get; set; }

    // Many-to-many with Tag through the join entity PostTag
    public ICollection<PostTag>? PostTags { get; set; }

    public ICollection<Like>? Likes { get; set; }
    public ICollection<Bookmark>? Bookmarks { get; set; }

    // URL of the image/video uploaded to Cloudinary. Nullable because media is optional.
    public string? ImageUrl { get; set; }
}
