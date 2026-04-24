using Blog.Domain.Entities;

namespace Blog.Domain.Interfaces;

public interface ICommentRepository
{
    // Returns all comments for a post
    Task<IEnumerable<Comment>> GetByPostIdAsync(int postId);

    // Returns a comment by its ID
    Task<Comment?> GetByIdAsync(int id);

    // Returns a comment with its associated post, used for authorization checks
    Task<Comment?> GetByIdWithPostAsync(int id);
    
    Task AddAsync(Comment comment);
    Task DeleteAsync(Comment comment);

    // Returns the total number of comments for a post (including replies)
    Task<int> GetCountByPostIdAsync(int postId);
}
