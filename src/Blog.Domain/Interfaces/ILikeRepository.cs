using Blog.Domain.Entities;

namespace Blog.Domain.Interfaces;

public interface ILikeRepository
{
    // Checks if a user already liked a post
    Task<Like?> GetAsync(int postId, int userId);
    Task<int> GetCountByPostIdAsync(int postId);
    Task AddAsync(Like like);
    Task DeleteAsync(Like like);
}
