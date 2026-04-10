namespace Blog.Domain.Entities;

// Entity class — represents the "Tags" table in the database.
// Tags are hashtags extracted from post content (e.g., #dev, #react).
public class Tag
{
    // Primary Key — auto-incremented
    public int Id { get; set; }

    // The hashtag name without the '#' symbol, stored in lowercase (e.g., "dev", "react")
    public required string Name { get; set; }

    // Navigation property for the many-to-many relationship with Post.
    // A tag can belong to many posts. The join table is PostTag.
    public ICollection<PostTag>? PostTags { get; set; }
}
