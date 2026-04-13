using Blog.Domain.Entities;
using Blog.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Blog.Infrastructure.Repositories;

// Concrete implementation of ILikeRepository using EF Core.
// Like uses a composite PK (PostId, UserId) — no separate Id column.
// The toggle pattern (like/unlike) is handled at the service layer;
// this repository only provides the CRUD primitives.
public class LikeRepository : ILikeRepository
{
    private readonly AppDbContext _context;

    public LikeRepository(AppDbContext context)
    {
        _context = context;
    }

    // Finds a like by its composite key (PostId + UserId).
    // Returns null if the user hasn't liked this post — used by the service to decide add vs. remove.
    public async Task<Like?> GetAsync(int postId, int userId)
    {
        return await _context.Likes
            .FirstOrDefaultAsync(l => l.PostId == postId && l.UserId == userId);
    }

    // CountAsync — translates to SQL: SELECT COUNT(*) FROM "Likes" WHERE "PostId" = @postId
    // More efficient than loading all entities and counting in memory.
    public async Task<int> GetCountByPostIdAsync(int postId)
    {
        return await _context.Likes
            .CountAsync(l => l.PostId == postId);
    }

    public async Task<List<Like>> GetByPostIdWithUsersAsync(int postId)
    {
        return await _context.Likes
            .Include(l => l.User)
            .Where(l => l.PostId == postId)
            .OrderByDescending(l => l.CreatedAt)
            .ToListAsync();
    }

    public async Task AddAsync(Like like)
    {
        await _context.Likes.AddAsync(like);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Like like)
    {
        _context.Likes.Remove(like);
        await _context.SaveChangesAsync();
    }
}
