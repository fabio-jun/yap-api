namespace Blog.Domain.Entities;

// Uses a composite PK (FollowerId + FollowedId) to ensure unique follow relationships.
public class Follow
{
    public int FollowerId { get; set; }
    // Navigation property — when you access follow.Follower, EF Core resolves it via JOIN:
    // SELECT * FROM Users WHERE Id = follow.FollowerId
    public User? Follower { get; set; }

    // The user being followed.
    public int FollowedId { get; set; }
    public User? Followed { get; set; }

    public DateTime CreatedAt { get; set; }
}
