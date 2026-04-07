namespace Blog.Domain.Entities;

public class Follow
{
    public int FollowerId { get; set; }
    // When you access follow.Follower, EF Core does something like SELECT * FROM Users WHERE Id = follow.FollowerId
    public User? Follower { get; set; }  
    public int FollowingId { get; set; }
    public User? Following { get; set; }
    public DateTime CreatedAt { get; set; }
}
