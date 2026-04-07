using Blog.Domain.Entities;
using Blog.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Blog.Infrastructure.Repositories;

public class FollowRepository : IFollowRepository
{
    private readonly AppDbContext _context;

    public FollowRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Follow?> GetAsync(int followerId, int followingId)
    {
        return await _context.Follows
            .FirstOrDefaultAsync(f => f.FollowerId == followerId && f.FollowingId == followingId);
    }

    public async Task<IEnumerable<User>> GetFollowersAsync(int userId)
    {
        return await _context.Follows
            .Where(f => f.FollowingId == userId)
            .Select(f => f.Follower!)
            .ToListAsync();
    }

    public async Task<IEnumerable<User>> GetFollowingAsync(int userId)
    {
        return await _context.Follows
            .Where(f => f.FollowerId == userId)
            .Select(f => f.Following!)
            .ToListAsync();
    }

    public async Task<int> GetFollowersCountAsync(int userId)
    {
        return await _context.Follows.CountAsync(f => f.FollowingId == userId);
    }

    public async Task<int> GetFollowingCountAsync(int userId)
    {
        return await _context.Follows.CountAsync(f => f.FollowerId == userId);
    }

    public async Task AddAsync(Follow follow)
    {
        await _context.Follows.AddAsync(follow);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Follow follow)
    {
        _context.Follows.Remove(follow);
        await _context.SaveChangesAsync();
    }
}
