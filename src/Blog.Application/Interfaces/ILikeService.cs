namespace Blog.Application.Interfaces;

public interface ILikeService
{
    // Toggles like
    Task<bool> ToggleLikeAsync(int postId, int userId);
    // Returns the number of likes for a post
    Task<int> GetCountAsync(int postId);
    // Checks if a user has liked a post
    Task<bool> HasLikedAsync(int postId, int userId);
}
