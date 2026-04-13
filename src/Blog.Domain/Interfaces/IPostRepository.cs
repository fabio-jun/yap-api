using Blog.Domain.Entities;

namespace Blog.Domain.Interfaces;


public interface IPostRepository
{
    Task<IEnumerable<Post>> GetAllAsync();
    Task<IEnumerable<Post>> GetByUserIdAsync(int userId);

    // Returns a tuple for pagination.
    Task<(IEnumerable<Post> Items, int TotalCount)> GetAllPagedAsync(int page, int pageSize);

    // Returns posts from users that the given userId follows to form the feed
    Task<IEnumerable<Post>> GetFeedAsync(int userId);

    // Returns full-text search results on post content (uses PostgreSQL ILike for case-insensitive matching)
    Task<IEnumerable<Post>> SearchAsync(string query);

    // Filter posts by hashtag
    Task<IEnumerable<Post>> GetByTagAsync(string tagName);

    Task<Post?> GetByIdAsync(int id);
    Task AddAsync(Post post);
    Task UpdateAsync(Post post);
    Task DeleteAsync(Post post);
}
