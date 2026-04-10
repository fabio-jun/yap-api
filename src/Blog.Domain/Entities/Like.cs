namespace Blog.Domain.Entities;

// Entity class — represents the "Likes" table in the database.
// Uses a composite primary key (PostId + UserId) to ensure a user can only like a post once.
// No separate Id column — the combination of PostId and UserId IS the primary key.
public class Like
{
    // Part 1 of composite PK — references Posts.Id
    public int PostId { get; set; }
    public Post? Post { get; set; }

    // Part 2 of composite PK — references Users.Id
    public int UserId { get; set; }
    public User? User { get; set; }

    public DateTime CreatedAt { get; set; }
}
