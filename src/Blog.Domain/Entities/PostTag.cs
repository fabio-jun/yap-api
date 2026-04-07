namespace Blog.Domain.Entities;

//Join table for the many-to-many relationship between Post and Tag
// A post can have many tags, and a tag can belong to many posts
public class PostTag
{
    public int PostId { get; set; }
    public Post? Post { get; set; }

    public int TagId { get; set; }
    public Tag? Tag { get; set; }
}