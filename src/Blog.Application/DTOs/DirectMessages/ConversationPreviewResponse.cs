namespace Blog.Application.DTOs.DirectMessages;

public class ConversationPreviewResponse
{
    public int UserId { get; set; }
    public required string UserName { get; set; }
    public string? ProfileImageUrl { get; set; }
    public required string LastMessageContent { get; set; }
    public DateTime LastMessageDate { get; set; }
}
