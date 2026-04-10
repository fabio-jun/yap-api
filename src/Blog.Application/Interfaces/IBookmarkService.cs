using Blog.Application.DTOs.Posts;

namespace Blog.Application.Interfaces;

// Service interface for bookmark operations.
public interface IBookmarkService
{
    // Toggles bookmark on a post. Returns true if now bookmarked, false if removed.
    Task<bool> ToggleBookmarkAsync(int postId, int userId);

    // Checks if a user has bookmarked a post
    Task<bool> HasBookmarkedAsync(int postId, int userId);

    // Returns all posts bookmarked by a user (for the Bookmarks page)
    Task<IEnumerable<PostResponse>> GetBookmarksAsync(int userId);
}
