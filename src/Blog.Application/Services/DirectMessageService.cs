using Blog.Domain.Entities;
using Blog.Domain.Interfaces;
using Blog.Application.DTOs.DirectMessages;
using Blog.Application.Interfaces;

namespace Blog.Application.Services;

public class DirectMessageService : IDirectMessageService
{
    private readonly IDirectMessageRepository _messageRepository;

    public DirectMessageService(IDirectMessageRepository messageRepository)
    {
        _messageRepository = messageRepository;
    }

    public async Task<IEnumerable<ConversationPreviewResponse>> GetConversationsAsync(int userId)
    {
        var messages = await _messageRepository.GetConversationsListAsync(userId);

        return messages.Select(m =>
        {
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

    public async Task<DirectMessageResponse> SendAsync(int senderId, int receiverId, SendMessageRequest request)
    {
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

    public async Task DeleteAsync(int messageId, int userId)
    {
        var message = await _messageRepository.GetByIdAsync(messageId);
        if (message == null)
            throw new KeyNotFoundException("Message not found.");

        if (message.SenderId != userId)
            throw new UnauthorizedAccessException("You can only delete your own messages.");

        await _messageRepository.DeleteAsync(message);
    }
}
