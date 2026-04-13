namespace Blog.Domain.Entities;

public class BlockedUser
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; }

    // User that is blocking
    public int BlockerId { get; set; }
    public User? Blocker { get; set; }

    // User that is being blocked
    public int BlockedId { get; set; }
    public User? Blocked { get; set; }
}
