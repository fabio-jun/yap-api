using Blog.Domain.Entities;
using Blog.Domain.Interfaces;
using Blog.Application.DTOs.Users;
using Blog.Application.Interfaces;

namespace Blog.Application.Services;

// Service that handles follow relationships and follow notifications.
public class FollowService : IFollowService
{
    private readonly IFollowRepository _followRepository;
    private readonly INotificationService _notificationService;

    // Repository and notification service are injected by ASP.NET Core's DI container.
    public FollowService(
        IFollowRepository followRepository,
        INotificationService notificationService)
    {
        _followRepository = followRepository;
        _notificationService = notificationService;
    }

    // Toggles a follow relationship. Returns true when following, false when unfollowed.
    public async Task<bool> ToggleFollowAsync(int followerId, int followedId)
    {
        // Business rule: users cannot follow themselves.
        if (followerId == followedId)
            throw new ArgumentException("You cannot follow yourself.");

        var existing = await _followRepository.GetAsync(followerId, followedId);

        if (existing != null)
        {
            // Unfollow also removes the matching follow notification.
            await _followRepository.DeleteAsync(existing);
            await _notificationService.DeleteNotificationAsync(
                NotificationType.Follow, followerId, followedId, null);
            return false;
        }

        var follow = new Follow
        {
            FollowerId = followerId,
            FollowedId = followedId,
            CreatedAt = DateTime.UtcNow
        };

        await _followRepository.AddAsync(follow);
        // Notify the followed user that they gained a follower.
        await _notificationService.CreateNotificationAsync(
            NotificationType.Follow, followerId, followedId, null);
        return true;
    }

    // Returns the users who follow the given user.
    public async Task<IEnumerable<UserResponse>> GetFollowersAsync(int userId)
    {
        var users = await _followRepository.GetFollowersAsync(userId);
        // Map user entities to public profile DTOs.
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

    // Returns the users followed by the given user.
    public async Task<IEnumerable<UserResponse>> GetFollowingAsync(int userId)
    {
        var users = await _followRepository.GetFollowingAsync(userId);
        // Map user entities to public profile DTOs.
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

    // Checks whether followerId is currently following followedId.
    public async Task<bool> IsFollowingAsync(int followerId, int followedId)
    {
        var follow = await _followRepository.GetAsync(followerId, followedId);
        return follow != null;
    }
}
