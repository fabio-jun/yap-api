using Blog.Domain.Entities;
using Blog.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Blog.Infrastructure.Repositories;

public class RepostRepository : IRepostRepository
{
    private readonly AppDbContext _context;

    public RepostRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Repost?> GetAsync(int userId, int postId)
    {
        return await _context.Reposts
            .Include(r => r.User)
            .Include(r => r.Post)
                .ThenInclude(p => p!.Author)
            .FirstOrDefaultAsync(r => r.UserId == userId && r.PostId == postId);
    }

    public async Task<List<Repost>> GetAllAsync()
    {
        return await BaseQuery()
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<Repost>> GetByUserIdAsync(int userId)
    {
        return await BaseQuery()
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<Repost>> GetFeedAsync(int userId)
    {
        return await BaseQuery()
            .Where(r => _context.Follows.Any(f => f.FollowerId == userId && f.FollowedId == r.UserId))
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<int> GetCountByPostIdAsync(int postId)
    {
        return await _context.Reposts.CountAsync(r => r.PostId == postId);
    }

    public async Task<Dictionary<int, int>> GetCountsByPostIdsAsync(IEnumerable<int> postIds)
    {
        var ids = postIds.Distinct().ToList();
        return await _context.Reposts
            .Where(r => ids.Contains(r.PostId))
            .GroupBy(r => r.PostId)
            .Select(g => new { PostId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.PostId, x => x.Count);
    }

    public async Task<HashSet<int>> GetRepostedPostIdsAsync(int userId, IEnumerable<int> postIds)
    {
        var ids = postIds.Distinct().ToList();
        var reposted = await _context.Reposts
            .Where(r => r.UserId == userId && ids.Contains(r.PostId))
            .Select(r => r.PostId)
            .ToListAsync();

        return reposted.ToHashSet();
    }

    public async Task AddAsync(Repost repost)
    {
        await _context.Reposts.AddAsync(repost);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Repost repost)
    {
        _context.Reposts.Update(repost);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Repost repost)
    {
        _context.Reposts.Remove(repost);
        await _context.SaveChangesAsync();
    }

    private IQueryable<Repost> BaseQuery()
    {
        return _context.Reposts
            .Include(r => r.User)
            .Include(r => r.Post)
                .ThenInclude(p => p!.Author);
    }
}
