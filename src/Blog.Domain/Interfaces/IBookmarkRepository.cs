using Blog.Domain.Entities;

namespace Blog.Domain.Interfaces;

public interface IBookmarkRepository
{
    Task<Bookmark?> GetAsync(int postId, int userId);
    Task<IEnumerable<Post>> GetByUserAsync(int userId);
    Task AddAsync(Bookmark bookmark);
    Task DeleteAsync(Bookmark bookmark);
}
