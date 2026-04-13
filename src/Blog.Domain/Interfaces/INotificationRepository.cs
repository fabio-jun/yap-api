using Blog.Domain.Entities;

namespace Blog.Domain.Interfaces;

public interface INotificationRepository
{
    // Get notification by ID
    Task<Notification?> GetByIdAsync(int id);
    // Return notifications for a user with pagination
    Task<List<Notification>> GetByUserIdAsync(int userId, int page, int pageSize);
    // Return total count of notifications for a user
    Task<int> GetTotalCountAsync(int userId);
    // Return recent notifications for a user
    Task<List<Notification>> GetRecentByUserIdAsync(int userId, int count);
    // Return count of unread notifications for a user
    Task<int> GetUnreadCountAsync(int userId);

    Task CreateAsync(Notification notification);
    Task MarkAsReadAsync(int id, int userId);
    Task MarkAllAsReadAsync(int userId);
    Task DeleteAsync(int id, int userId);
    // Check if a similar notification already exists to prevent duplicates
    Task<bool> ExistsAsync(NotificationType type, int actorId, int userId, int? postId);
    Task DeleteByTypeAsync(NotificationType type, int actorId, int userId, int? postId);
}
