using Blog.Application.DTOs;
using Blog.Application.DTOs.Notifications;
using Blog.Application.Cache;
using Blog.Application.Interfaces;
using Blog.Domain.Entities;
using Blog.Domain.Interfaces;

namespace Blog.Application.Services;

// Service that handles notification reads, lists, creation, deletion, and short-lived cache.
public class NotificationService : INotificationService
{
    // Notifications change frequently, so the cache intentionally uses a short TTL.
    private static readonly TimeSpan NotificationsTtl = TimeSpan.FromSeconds(15);

    private readonly INotificationRepository _notificationRepository;
    private readonly ICacheService _cache;

    // Repository and cache abstraction are injected by ASP.NET Core's DI container.
    public NotificationService(INotificationRepository notificationRepository, ICacheService cache)
    {
        _notificationRepository = notificationRepository;
        _cache = cache;
    }

    // Returns paged notifications for the user, cached briefly for the notifications page.
    public async Task<PagedResponse<NotificationResponse>> GetNotificationsAsync(int userId, int page, int pageSize)
    {
        var cacheKey = CacheKeys.Notifications(userId);
        var cached = await _cache.GetAsync<PagedResponse<NotificationResponse>>(cacheKey);
        if (cached != null)
        {
            return cached;
        }

        var notifications = await _notificationRepository.GetByUserIdAsync(userId, page, pageSize);
        var totalCount = await _notificationRepository.GetTotalCountAsync(userId);

        var result = new PagedResponse<NotificationResponse>
        {
            Items = notifications.Select(MapToResponse),
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };

        await _cache.SetAsync(cacheKey, result, NotificationsTtl);
        return result;
    }

    // Returns a small recent list for the notification dropdown.
    public async Task<List<NotificationResponse>> GetRecentAsync(int userId, int count)
    {
        var notifications = await _notificationRepository.GetRecentByUserIdAsync(userId, count);
        return notifications.Select(MapToResponse).ToList();
    }

    // Returns the unread badge count for the authenticated user.
    public async Task<int> GetUnreadCountAsync(int userId)
    {
        return await _notificationRepository.GetUnreadCountAsync(userId);
    }

    // Marks one notification as read, scoped to the owner user.
    public async Task MarkAsReadAsync(int id, int userId)
    {
        await _notificationRepository.MarkAsReadAsync(id, userId);
    }

    // Marks every notification for a user as read.
    public async Task MarkAllAsReadAsync(int userId)
    {
        await _notificationRepository.MarkAllAsReadAsync(userId);
    }

    // Deletes one notification owned by the user.
    public async Task DeleteAsync(int id, int userId)
    {
        await _notificationRepository.DeleteAsync(id, userId);
    }

    // Creates a notification unless it is a self-action or an existing duplicate.
    public async Task CreateNotificationAsync(NotificationType type, int actorId, int userId, int? postId)
    {
        // Users should not be notified about their own actions.
        if (actorId == userId) return;

        // Prevent duplicate notifications for the same type, actor, target user, and yap.
        var exists = await _notificationRepository.ExistsAsync(type, actorId, userId, postId);
        if (exists) return;

        var notification = new Notification
        {
            Type = type,
            ActorId = actorId,
            UserId = userId,
            PostId = postId,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };

        await _notificationRepository.CreateAsync(notification);
    }

    // Removes a notification that matches an undone action, such as unlike or unfollow.
    public async Task DeleteNotificationAsync(NotificationType type, int actorId, int userId, int? postId)
    {
        await _notificationRepository.DeleteByTypeAsync(type, actorId, userId, postId);
    }

    // Maps entity data into the UI-friendly notification DTO.
    private static NotificationResponse MapToResponse(Notification n)
    {
        return new NotificationResponse
        {
            Id = n.Id,
            Type = n.Type.ToString().ToLower(),
            IsRead = n.IsRead,
            CreatedAt = n.CreatedAt,
            ActorId = n.ActorId,
            ActorUsername = n.Actor?.UserName ?? string.Empty,
            ActorDisplayName = n.Actor?.DisplayName,
            ActorProfileImageUrl = n.Actor?.ProfileImageUrl,
            PostId = n.PostId,
            // Keep previews compact for dropdown/list rendering.
            PostContentPreview = n.Post?.Content != null
                ? (n.Post.Content.Length > 50 ? n.Post.Content[..50] + "..." : n.Post.Content)
                : null
        };
    }
}
