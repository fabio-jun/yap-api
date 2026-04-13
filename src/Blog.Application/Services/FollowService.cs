using Blog.Domain.Entities;
using Blog.Domain.Interfaces;
using Blog.Application.DTOs.Users;
using Blog.Application.Interfaces;

namespace Blog.Application.Services;

public class FollowService : IFollowService
{
    private readonly IFollowRepository _followRepository;
    private readonly INotificationService _notificationService;

    public FollowService(
        IFollowRepository followRepository,
        INotificationService notificationService)
    {
        _followRepository = followRepository;
        _notificationService = notificationService;
    }

    public async Task<bool> ToggleFollowAsync(int followerId, int followedId)
    {
        if (followerId == followedId)
            throw new ArgumentException("You cannot follow yourself.");

        var existing = await _followRepository.GetAsync(followerId, followedId);

        if (existing != null)
        {
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
        await _notificationService.CreateNotificationAsync(
            NotificationType.Follow, followerId, followedId, null);
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

    public async Task<bool> IsFollowingAsync(int followerId, int followedId)
    {
        var follow = await _followRepository.GetAsync(followerId, followedId);
        return follow != null;
    }
}
