using Blog.Domain.Entities;
using Blog.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Blog.Infrastructure.Repositories;

public class LikeRepository : ILikeRepository
{
    private readonly AppDbContext _context;

    public LikeRepository(AppDbContext context)
    {
        _context = context;
    }

    // Finds a like by PostId + UserId
    public async Task<Like?> GetAsync(int postId, int userId)
    {
        return await _context.Likes
            .FirstOrDefaultAsync(l => l.PostId == postId && l.UserId == userId);
    }

    // Counts the number of likes
    public async Task<int> GetCountByPostIdAsync(int postId)
    {
        return await _context.Likes
            .CountAsync(l => l.PostId == postId);
    }

    // Adds a new like
    public async Task AddAsync(Like like)
    {
        await _context.Likes.AddAsync(like);
        await _context.SaveChangesAsync();
    }

    // Removes a like
    public async Task DeleteAsync(Like like)
    {
        _context.Likes.Remove(like);
        await _context.SaveChangesAsync();
    }
}
