using Blog.Domain.Entities;
using Blog.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Blog.Infrastructure.Repositories;

public class NotificationRepository : INotificationRepository
{
    private readonly AppDbContext _context;

    public NotificationRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Notification?> GetByIdAsync(int id)
    {
        return await _context.Notifications
            .Include(n => n.Actor)
            .Include(n => n.Post)
            .FirstOrDefaultAsync(n => n.Id == id);
    }

    public async Task<List<Notification>> GetByUserIdAsync(int userId, int page, int pageSize)
    {
        return await _context.Notifications
            .Where(n => n.UserId == userId)
            .Include(n => n.Actor)
            .Include(n => n.Post)
            .OrderByDescending(n => n.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> GetTotalCountAsync(int userId)
    {
        return await _context.Notifications
            .CountAsync(n => n.UserId == userId);
    }

    public async Task<List<Notification>> GetRecentByUserIdAsync(int userId, int count)
    {
        return await _context.Notifications
            .Where(n => n.UserId == userId)
            .Include(n => n.Actor)
            .Include(n => n.Post)
            .OrderByDescending(n => n.CreatedAt)
            .Take(count)
            .ToListAsync();
    }

    public async Task<int> GetUnreadCountAsync(int userId)
    {
        return await _context.Notifications
            .CountAsync(n => n.UserId == userId && !n.IsRead);
    }

    public async Task CreateAsync(Notification notification)
    {
        await _context.Notifications.AddAsync(notification);
        await _context.SaveChangesAsync();
    }

    public async Task MarkAsReadAsync(int id, int userId)
    {
        await _context.Notifications
            .Where(n => n.Id == id && n.UserId == userId)
            .ExecuteUpdateAsync(s => s.SetProperty(n => n.IsRead, true));
    }

    public async Task MarkAllAsReadAsync(int userId)
    {
        await _context.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .ExecuteUpdateAsync(s => s.SetProperty(n => n.IsRead, true));
    }

    public async Task DeleteAsync(int id, int userId)
    {
        await _context.Notifications
            .Where(n => n.Id == id && n.UserId == userId)
            .ExecuteDeleteAsync();
    }

    public async Task<bool> ExistsAsync(NotificationType type, int actorId, int userId, int? postId)
    {
        return await _context.Notifications
            .AnyAsync(n => n.Type == type && n.ActorId == actorId && n.UserId == userId && n.PostId == postId);
    }

    public async Task DeleteByTypeAsync(NotificationType type, int actorId, int userId, int? postId)
    {
        await _context.Notifications
            .Where(n => n.Type == type && n.ActorId == actorId && n.UserId == userId && n.PostId == postId)
            .ExecuteDeleteAsync();
    }
}
