using Blog.Domain.Entities;

namespace Blog.Domain.Interfaces;

// Repository interface for Like data access.
public interface ILikeRepository
{
    // Checks if a user already liked a post. Returns null if no like exists.
    // Used by the toggle logic: if exists → unlike, if null → like.
    Task<Like?> GetAsync(int postId, int userId);

    // Returns the total number of likes on a post (SELECT COUNT(*))
    Task<int> GetCountByPostIdAsync(int postId);

    Task AddAsync(Like like);
    Task DeleteAsync(Like like);
}
