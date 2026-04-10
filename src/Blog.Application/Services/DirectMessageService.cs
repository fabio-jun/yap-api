using Blog.Domain.Entities;
using Blog.Domain.Interfaces;
using Blog.Application.DTOs.DirectMessages;
using Blog.Application.Interfaces;

namespace Blog.Application.Services;

// Service that handles direct messaging: inbox, conversation, send, delete.
public class DirectMessageService : IDirectMessageService
{
    private readonly IDirectMessageRepository _messageRepository;

    public DirectMessageService(IDirectMessageRepository messageRepository)
    {
        _messageRepository = messageRepository;
    }

    // Returns the inbox: one preview per conversation, showing the other user and last message.
    public async Task<IEnumerable<ConversationPreviewResponse>> GetConversationsAsync(int userId)
    {
        // GetConversationsListAsync returns the latest message from each conversation
        var messages = await _messageRepository.GetConversationsListAsync(userId);

        // .Select() with a block lambda — when the mapping logic needs local variables
        return messages.Select(m =>
        {
            // Determine who the "other" user is: if I sent the message, the other is the receiver
            var isOtherUser = m.SenderId == userId;
            var otherUser = isOtherUser ? m.Receiver : m.Sender;

            return new ConversationPreviewResponse
            {
                UserId = otherUser?.Id ?? 0,
                UserName = otherUser?.UserName ?? string.Empty,
                ProfileImageUrl = otherUser?.ProfileImageUrl,
                LastMessageContent = m.Content,
                LastMessageDate = m.CreatedAt
            };
        });
    }

    // Returns all messages between two users, ordered chronologically.
    public async Task<IEnumerable<DirectMessageResponse>> GetConversationAsync(int currentUserId, int otherUserId)
    {
        var messages = await _messageRepository.GetConversationAsync(currentUserId, otherUserId);

        return messages.Select(m => new DirectMessageResponse
        {
            Id = m.Id,
            Content = m.Content,
            CreatedAt = m.CreatedAt,
            SenderId = m.SenderId,
            SenderName = m.Sender?.UserName ?? string.Empty,
            SenderProfileImageUrl = m.Sender?.ProfileImageUrl,
            ReceiverId = m.ReceiverId,
            ReceiverName = m.Receiver?.UserName ?? string.Empty,
            ReceiverProfileImageUrl = m.Receiver?.ProfileImageUrl
        });
    }

    // Sends a message: validates content, creates entity, persists.
    public async Task<DirectMessageResponse> SendAsync(int senderId, int receiverId, SendMessageRequest request)
    {
        // Input validation at the service boundary
        if (string.IsNullOrWhiteSpace(request.Content))
            throw new ArgumentException("Message content cannot be empty.");

        if (request.Content.Length > 2000)
            throw new ArgumentException("Message must be 2000 characters or less.");

        if (senderId == receiverId)
            throw new ArgumentException("Cannot send a message to yourself.");

        var message = new DirectMessage
        {
            Content = request.Content,
            CreatedAt = DateTime.UtcNow,
            SenderId = senderId,
            ReceiverId = receiverId
        };

        await _messageRepository.AddAsync(message);

        return new DirectMessageResponse
        {
            Id = message.Id,
            Content = message.Content,
            CreatedAt = message.CreatedAt,
            SenderId = message.SenderId,
            SenderName = string.Empty,
            ReceiverId = message.ReceiverId,
            ReceiverName = string.Empty
        };
    }

    // Deletes a message — only the sender can delete their own messages.
    public async Task DeleteAsync(int messageId, int userId)
    {
        var message = await _messageRepository.GetByIdAsync(messageId);
        if (message == null)
            throw new KeyNotFoundException("Message not found.");

        // Authorization: only the sender can delete
        if (message.SenderId != userId)
            throw new UnauthorizedAccessException("You can only delete your own messages.");

        await _messageRepository.DeleteAsync(message);
    }
}
