namespace Blog.Domain.Entities;

// Uses a composite primary key (PostId + UserId) 
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
