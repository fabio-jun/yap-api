namespace Blog.Domain.Entities;

// Entity class — represents the "Comments" table in the database.
// A comment belongs to one Post and one User (the author).
public class Comment
{
    // Primary Key — auto-incremented
    public int Id { get; set; }
    public required string Content { get; set; }
    public DateTime CreatedAt { get; set; }

    // Foreign Key — references Posts.Id
    public int PostId { get; set; }
    // Navigation property — doesn't create a column in the DB.
    // EF Core uses it to resolve the relationship when you do .Include(c => c.Post)
    public Post? Post { get; set; }

    // Foreign Key — references Users.Id
    public int AuthorId { get; set; }
    // Navigation property for the comment's author
    public User? Author { get; set; }
}
