using Blog.Application.DTOs.DirectMessages;

namespace Blog.Application.Interfaces;

public interface IDirectMessageService
{
    Task<IEnumerable<ConversationPreviewResponse>> GetConversationsAsync(int userId);
    Task<IEnumerable<DirectMessageResponse>> GetConversationAsync(int currentUserId, int otherUserId);
    Task<DirectMessageResponse> SendAsync(int senderId, int receiverId, SendMessageRequest request);
    Task DeleteAsync(int messageId, int userId);
}
