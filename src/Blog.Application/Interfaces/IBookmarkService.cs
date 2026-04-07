using Blog.Application.DTOs.Posts;

namespace Blog.Application.Interfaces;

public interface IBookmarkService
{
    Task<bool> ToggleBookmarkAsync(int postId, int userId);
    Task<bool> HasBookmarkedAsync(int postId, int userId);
    Task<IEnumerable<PostResponse>> GetBookmarksAsync(int userId);
}
