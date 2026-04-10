using Blog.Domain.Entities;

namespace Blog.Domain.Interfaces;

public interface IDirectMessageRepository
{
    Task<IEnumerable<DirectMessage>> GetConversationAsync(int userId1, int userId2);
    Task<IEnumerable<DirectMessage>> GetConversationsListAsync(int userId);
    Task<DirectMessage?> GetByIdAsync(int id);
    Task AddAsync(DirectMessage message);
    Task DeleteAsync(DirectMessage message);
}
