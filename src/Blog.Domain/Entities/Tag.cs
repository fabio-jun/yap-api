namespace Blog.Domain.Entities;

public class Tag
{
    public int Id { get; set; }
    public required string Name { get; set; }

    // Navigation property for the many-to-many relationship with Post
    public ICollection<PostTag>? PostTags { get; set; }
}