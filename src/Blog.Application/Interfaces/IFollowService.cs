using Blog.Application.DTOs.Users;

namespace Blog.Application.Interfaces;

// Service interface for follow operations.
public interface IFollowService
{
    // Toggles follow relationship. Returns true if now following, false if unfollowed.
    Task<bool> ToggleFollowAsync(int followerId, int followedId);

    // Returns list of users who follow the given user
    Task<IEnumerable<UserResponse>> GetFollowersAsync(int userId);

    // Returns list of users the given user follows
    Task<IEnumerable<UserResponse>> GetFollowingAsync(int userId);

    // Checks if followerId follows followedId
    Task<bool> IsFollowingAsync(int followerId, int followedId);
}
