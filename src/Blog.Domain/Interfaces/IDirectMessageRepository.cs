using Blog.Domain.Entities;

namespace Blog.Domain.Interfaces;


public interface IDirectMessageRepository
{
    // Returns all messages between two users, ordered chronologically, for chat view
    Task<IEnumerable<DirectMessage>> GetConversationAsync(int userId1, int userId2);

    // Returns the latest message from each conversation the user participates in, for inbox
    Task<IEnumerable<DirectMessage>> GetConversationsListAsync(int userId);

    // Finds a single message by ID (used for delete authorization)
    Task<DirectMessage?> GetByIdAsync(int id);

    Task AddAsync(DirectMessage message);
    Task DeleteAsync(DirectMessage message);
}
