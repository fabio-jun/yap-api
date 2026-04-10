namespace Blog.Domain.Entities;

// Entity class — represents the "Follows" table in the database.
// Uses a composite primary key (FollowerId + FollowingId) to ensure unique follow relationships.
// Models the relationship: "FollowerId follows FollowingId"
public class Follow
{
    // The user who is following someone
    public int FollowerId { get; set; }
    // Navigation property — when you access follow.Follower, EF Core resolves it via JOIN:
    // SELECT * FROM Users WHERE Id = follow.FollowerId
    public User? Follower { get; set; }

    // The user being followed
    public int FollowingId { get; set; }
    public User? Following { get; set; }

    public DateTime CreatedAt { get; set; }
}
