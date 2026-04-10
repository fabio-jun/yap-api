namespace Blog.Domain.Entities;

// Entity class — represents the "Bookmarks" table in the database.
// Uses a composite primary key (PostId + UserId) — a user can only bookmark a post once.
// Same pattern as Like: no separate Id, the pair IS the key.
public class Bookmark
{
    // Part 1 of composite PK — references Posts.Id
    public int PostId { get; set; }
    public Post? Post { get; set; }

    // Part 2 of composite PK — references Users.Id
    public int UserId { get; set; }
    public User? User { get; set; }

    public DateTime CreatedAt { get; set; }
}
