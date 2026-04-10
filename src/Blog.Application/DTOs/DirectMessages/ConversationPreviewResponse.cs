namespace Blog.Application.DTOs.DirectMessages;

// DTO for the messages inbox — shows a preview of each conversation.
// One entry per conversation, showing the other user's info and the last message.
public class ConversationPreviewResponse
{
    // The other user in the conversation (not the current user)
    public int UserId { get; set; }
    public required string UserName { get; set; }
    public string? ProfileImageUrl { get; set; }

    // Preview of the most recent message in the conversation
    public required string LastMessageContent { get; set; }
    public DateTime LastMessageDate { get; set; }
}
