namespace Blog.Application.DTOs.DirectMessages;

// DTO for sending a direct message.
// The receiverId comes from the route, senderId from the JWT.
public class SendMessageRequest
{
    // Message text — validated in DirectMessageService (max 2000 chars, not empty)
    public required string Content { get; set; }
}
