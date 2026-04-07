using Blog.Domain.Entities;
using Blog.Domain.Interfaces;
using Blog.Application.DTOs.Users;
using Blog.Application.Interfaces;

namespace Blog.Application.Services;

public class FollowService : IFollowService
{
    private readonly IFollowRepository _followRepository;

    public FollowService(IFollowRepository followRepository)
    {
        _followRepository = followRepository;
    }

    // Returns true if now following, false if unfollowed
    public async Task<bool> ToggleFollowAsync(int followerId, int followingId)
    {
        if (followerId == followingId)
            throw new ArgumentException("You cannot follow yourself.");

        var existing = await _followRepository.GetAsync(followerId, followingId);

        if (existing != null)
        {
            await _followRepository.DeleteAsync(existing);
            return false;
        }

        var follow = new Follow
        {
            FollowerId = followerId,
            FollowingId = followingId,
            CreatedAt = DateTime.UtcNow
        };

        await _followRepository.AddAsync(follow);
        return true;
    }

    public async Task<IEnumerable<UserResponse>> GetFollowersAsync(int userId)
    {
        var users = await _followRepository.GetFollowersAsync(userId);
        return users.Select(u => new UserResponse
        {
            Id = u.Id,
            UserName = u.UserName,
            Email = u.Email,
            Role = u.Role,
            CreatedAt = u.CreatedAt,
            ProfileImageUrl = u.ProfileImageUrl
        });
    }

    public async Task<IEnumerable<UserResponse>> GetFollowingAsync(int userId)
    {
        var users = await _followRepository.GetFollowingAsync(userId);
        return users.Select(u => new UserResponse
        {
            Id = u.Id,
            UserName = u.UserName,
            Email = u.Email,
            Role = u.Role,
            CreatedAt = u.CreatedAt,
            ProfileImageUrl = u.ProfileImageUrl
        });
    }

    public async Task<bool> IsFollowingAsync(int followerId, int followingId)
    {
        var follow = await _followRepository.GetAsync(followerId, followingId);
        return follow != null;
    }
}
