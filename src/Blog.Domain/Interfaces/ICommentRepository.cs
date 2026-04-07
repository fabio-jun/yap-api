using Blog.Domain.Entities;

namespace Blog.Domain.Interfaces;

public interface ICommentRepository
{
    Task<IEnumerable<Comment>> GetByPostIdAsync(int postId);
    Task<Comment?> GetByIdAsync(int id);
    Task AddAsync(Comment comment);
    Task DeleteAsync(Comment comment);
}