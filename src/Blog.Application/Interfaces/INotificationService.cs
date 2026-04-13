using Blog.Application.DTOs;
using Blog.Application.DTOs.Notifications;
using Blog.Domain.Entities;

namespace Blog.Application.Interfaces;

public interface INotificationService
{
    Task<PagedResponse<NotificationResponse>> GetNotificationsAsync(int userId, int page, int pageSize);
    Task<List<NotificationResponse>> GetRecentAsync(int userId, int count);
    Task<int> GetUnreadCountAsync(int userId);
    Task MarkAsReadAsync(int id, int userId);
    Task MarkAllAsReadAsync(int userId);
    Task DeleteAsync(int id, int userId);
    Task CreateNotificationAsync(NotificationType type, int actorId, int userId, int? postId);
    Task DeleteNotificationAsync(NotificationType type, int actorId, int userId, int? postId);
}
