namespace Blog.Application.DTOs.DirectMessages;

public class DirectMessageResponse
{
    public int Id { get; set; }
    public required string Content { get; set; }
    public DateTime CreatedAt { get; set; }
    public int SenderId { get; set; }
    public required string SenderName { get; set; }
    public string? SenderProfileImageUrl { get; set; }
    public int ReceiverId { get; set; }
    public required string ReceiverName { get; set; }
    public string? ReceiverProfileImageUrl { get; set; }
}
