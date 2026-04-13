namespace Blog.Domain.Entities;

// Join entity for the many-to-many relationship between Post and Tag.
// Uses a composite primary key (PostId + TagId) configured in PostTagConfiguration.
public class PostTag
{
    // FK to Posts table
    public int PostId { get; set; }
    public Post? Post { get; set; }

    // FK to Tags table
    public int TagId { get; set; }
    public Tag? Tag { get; set; }
}
