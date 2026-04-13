namespace Blog.Application.DTOs.Notifications;

public class NotificationResponse
{
    public int Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
    public int ActorId { get; set; }
    public string ActorUsername { get; set; } = string.Empty;
    public string? ActorDisplayName { get; set; }
    public string? ActorProfileImageUrl { get; set; }
    public int? PostId { get; set; }
    public string? PostContentPreview { get; set; }
}
