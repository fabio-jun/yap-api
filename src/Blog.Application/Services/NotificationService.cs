using Blog.Application.DTOs;
using Blog.Application.DTOs.Notifications;
using Blog.Application.Interfaces;
using Blog.Domain.Entities;
using Blog.Domain.Interfaces;

namespace Blog.Application.Services;

public class NotificationService : INotificationService
{
    private readonly INotificationRepository _notificationRepository;

    public NotificationService(INotificationRepository notificationRepository)
    {
        _notificationRepository = notificationRepository;
    }

    public async Task<PagedResponse<NotificationResponse>> GetNotificationsAsync(int userId, int page, int pageSize)
    {
        var notifications = await _notificationRepository.GetByUserIdAsync(userId, page, pageSize);
        var totalCount = await _notificationRepository.GetTotalCountAsync(userId);

        return new PagedResponse<NotificationResponse>
        {
            Items = notifications.Select(MapToResponse),
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<List<NotificationResponse>> GetRecentAsync(int userId, int count)
    {
        var notifications = await _notificationRepository.GetRecentByUserIdAsync(userId, count);
        return notifications.Select(MapToResponse).ToList();
    }

    public async Task<int> GetUnreadCountAsync(int userId)
    {
        return await _notificationRepository.GetUnreadCountAsync(userId);
    }

    public async Task MarkAsReadAsync(int id, int userId)
    {
        await _notificationRepository.MarkAsReadAsync(id, userId);
    }

    public async Task MarkAllAsReadAsync(int userId)
    {
        await _notificationRepository.MarkAllAsReadAsync(userId);
    }

    public async Task DeleteAsync(int id, int userId)
    {
        await _notificationRepository.DeleteAsync(id, userId);
    }

    public async Task CreateNotificationAsync(NotificationType type, int actorId, int userId, int? postId)
    {
        if (actorId == userId) return;

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

    public async Task DeleteNotificationAsync(NotificationType type, int actorId, int userId, int? postId)
    {
        await _notificationRepository.DeleteByTypeAsync(type, actorId, userId, postId);
    }

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
            PostContentPreview = n.Post?.Content != null
                ? (n.Post.Content.Length > 50 ? n.Post.Content[..50] + "..." : n.Post.Content)
                : null
        };
    }
}
