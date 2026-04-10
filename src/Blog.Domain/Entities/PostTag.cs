namespace Blog.Domain.Entities;

// Join entity for the many-to-many relationship between Post and Tag.
// A post can have many tags, and a tag can belong to many posts.
// Uses a composite primary key (PostId + TagId) configured in PostTagConfiguration.
// This is the explicit join table pattern — EF Core also supports implicit many-to-many,
// but explicit gives more control (e.g., adding extra columns like CreatedAt if needed).
public class PostTag
{
    // FK to Posts table
    public int PostId { get; set; }
    public Post? Post { get; set; }

    // FK to Tags table
    public int TagId { get; set; }
    public Tag? Tag { get; set; }
}
