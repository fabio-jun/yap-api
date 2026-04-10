using Blog.Domain.Entities;

namespace Blog.Domain.Interfaces;

// Repository interface for Post data access.
// Defines all query and write operations the Application layer needs.
public interface IPostRepository
{
    Task<IEnumerable<Post>> GetAllAsync();
    Task<IEnumerable<Post>> GetByUserIdAsync(int userId);

    // Returns a tuple (Items, TotalCount) for pagination.
    // Tuple syntax: (Type1 Name1, Type2 Name2) — a lightweight way to return multiple values.
    Task<(IEnumerable<Post> Items, int TotalCount)> GetAllPagedAsync(int page, int pageSize);

    // Feed: posts from users that the given userId follows
    Task<IEnumerable<Post>> GetFeedAsync(int userId);

    // Full-text search on post content (uses PostgreSQL ILike for case-insensitive matching)
    Task<IEnumerable<Post>> SearchAsync(string query);

    // Filter posts by hashtag
    Task<IEnumerable<Post>> GetByTagAsync(string tagName);

    Task<Post?> GetByIdAsync(int id);
    Task AddAsync(Post post);
    Task UpdateAsync(Post post);
    Task DeleteAsync(Post post);
}
