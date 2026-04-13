using Blog.Domain.Entities;

namespace Blog.Domain.Interfaces;

public interface ILikeRepository
{
    // Checks if a user already liked a post. Returns null if no like exists.
    Task<Like?> GetAsync(int postId, int userId);

    // Returns the total number of likes on a post (SELECT COUNT(*))
    Task<int> GetCountByPostIdAsync(int postId);

    // Returns the list of Like objects for a post, and the User who liked
    Task<List<Like>> GetByPostIdWithUsersAsync(int postId);

    Task AddAsync(Like like);
    Task DeleteAsync(Like like);
}
