using Blog.Application.DTOs.Users;

namespace Blog.Application.Interfaces;

public interface IFollowService
{
    Task<bool> ToggleFollowAsync(int followerId, int followingId);
    Task<IEnumerable<UserResponse>> GetFollowersAsync(int userId);
    Task<IEnumerable<UserResponse>> GetFollowingAsync(int userId);
    Task<bool> IsFollowingAsync(int followerId, int followingId);
}
