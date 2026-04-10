using Blog.Domain.Entities;
using Blog.Domain.Interfaces;
using Blog.Application.DTOs.Users;
using Blog.Application.Interfaces;

namespace Blog.Application.Services;

// Service that handles follow/unfollow logic and follower queries.
public class FollowService : IFollowService
{
    private readonly IFollowRepository _followRepository;

    public FollowService(IFollowRepository followRepository)
    {
        _followRepository = followRepository;
    }

    // Toggles follow relationship — same pattern as LikeService.
    // Returns true if now following, false if unfollowed.
    public async Task<bool> ToggleFollowAsync(int followerId, int followingId)
    {
        // Prevent self-following
        if (followerId == followingId)
            throw new ArgumentException("You cannot follow yourself.");

        var existing = await _followRepository.GetAsync(followerId, followingId);

        if (existing != null)
        {
            // Already following — remove the relationship
            await _followRepository.DeleteAsync(existing);
            return false; // "unfollowed"
        }

        // Not following — create new Follow record
        var follow = new Follow
        {
            FollowerId = followerId,
            FollowingId = followingId,
            CreatedAt = DateTime.UtcNow
        };

        await _followRepository.AddAsync(follow);
        return true; // "followed"
    }

    // Returns list of users who follow the given user, mapped to DTOs.
    public async Task<IEnumerable<UserResponse>> GetFollowersAsync(int userId)
    {
        var users = await _followRepository.GetFollowersAsync(userId);
        // .Select() — LINQ projection, transforms User entities into UserResponse DTOs
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

    // Returns list of users the given user follows.
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

    // Checks if a follow relationship exists between two users.
    public async Task<bool> IsFollowingAsync(int followerId, int followingId)
    {
        var follow = await _followRepository.GetAsync(followerId, followingId);
        return follow != null;
    }
}
