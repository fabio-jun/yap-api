namespace Blog.Domain.Entities;

public enum NotificationType
{
    Like,
    Comment,
    Follow,
    Mention
}

public class Notification
{
    public int Id { get; set; }
    public NotificationType Type { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }

    public int UserId { get; set; }
    public User? User { get; set; }

    public int ActorId { get; set; }
    public User? Actor { get; set; }

    public int? PostId { get; set; }
    public Post? Post { get; set; }
}
