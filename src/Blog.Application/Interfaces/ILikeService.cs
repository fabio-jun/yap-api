namespace Blog.Application.Interfaces;

// Service interface for like operations.
// All methods use the toggle pattern: one endpoint handles both like and unlike.
public interface ILikeService
{
    // Toggles like on a post. Returns true if now liked, false if unliked.
    Task<bool> ToggleLikeAsync(int postId, int userId);

    // Returns the total number of likes for a post
    Task<int> GetCountAsync(int postId);

    // Checks if a specific user has liked a post (used for HasLiked in PostResponse)
    Task<bool> HasLikedAsync(int postId, int userId);
}
