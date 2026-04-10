using Blog.Domain.Entities;

namespace Blog.Domain.Interfaces;

// Repository interface for Comment data access.
public interface ICommentRepository
{
    // Returns all comments for a specific post
    Task<IEnumerable<Comment>> GetByPostIdAsync(int postId);

    // Returns a single comment by ID (used for delete authorization checks)
    Task<Comment?> GetByIdAsync(int id);

    Task AddAsync(Comment comment);
    Task DeleteAsync(Comment comment);
}
