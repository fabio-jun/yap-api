using Blog.Domain.Entities;

namespace Blog.Domain.Interfaces;

public interface IBookmarkRepository
{
    // Returns the check: if a user already bookmarked a post
    Task<Bookmark?> GetAsync(int postId, int userId);

    // Returns all posts bookmarked by a user, for the bookmarks page
    Task<IEnumerable<Post>> GetByUserAsync(int userId);
    Task AddAsync(Bookmark bookmark);
    Task DeleteAsync(Bookmark bookmark);
}
