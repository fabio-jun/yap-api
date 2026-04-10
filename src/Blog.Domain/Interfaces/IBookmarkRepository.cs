using Blog.Domain.Entities;

namespace Blog.Domain.Interfaces;

// Repository interface for Bookmark data access.
public interface IBookmarkRepository
{
    // Checks if a user already bookmarked a post (for toggle logic)
    Task<Bookmark?> GetAsync(int postId, int userId);

    // Returns all posts bookmarked by a user (for the Bookmarks page)
    Task<IEnumerable<Post>> GetByUserAsync(int userId);

    Task AddAsync(Bookmark bookmark);
    Task DeleteAsync(Bookmark bookmark);
}
