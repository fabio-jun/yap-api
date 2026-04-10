namespace Blog.Application.DTOs.DirectMessages;

// DTO for returning a single message in a conversation.
// Includes both sender and receiver info so the frontend can render chat bubbles.
public class DirectMessageResponse
{
    public int Id { get; set; }
    public required string Content { get; set; }
    public DateTime CreatedAt { get; set; }

    // Sender info — flattened from DirectMessage.Sender navigation property
    public int SenderId { get; set; }
    public required string SenderName { get; set; }
    public string? SenderProfileImageUrl { get; set; }

    // Receiver info — flattened from DirectMessage.Receiver navigation property
    public int ReceiverId { get; set; }
    public required string ReceiverName { get; set; }
    public string? ReceiverProfileImageUrl { get; set; }
}
