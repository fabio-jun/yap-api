using Blog.Application.DTOs.DirectMessages;

namespace Blog.Application.Interfaces;

// Service interface for direct message operations.
public interface IDirectMessageService
{
    // Returns the inbox: list of conversations with preview (last message) for each
    Task<IEnumerable<ConversationPreviewResponse>> GetConversationsAsync(int userId);

    // Returns all messages between two users, ordered chronologically
    Task<IEnumerable<DirectMessageResponse>> GetConversationAsync(int currentUserId, int otherUserId);

    // Sends a message from senderId to receiverId
    Task<DirectMessageResponse> SendAsync(int senderId, int receiverId, SendMessageRequest request);

    // Deletes a message — only the sender can delete their own messages
    Task DeleteAsync(int messageId, int userId);
}
