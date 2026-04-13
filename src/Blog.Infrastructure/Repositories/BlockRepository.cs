using Blog.Domain.Entities;
using Blog.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Blog.Infrastructure.Repositories;

public class BlockRepository : IBlockRepository
{
    private readonly AppDbContext _context;

    public BlockRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<BlockedUser?> GetAsync(int blockerId, int blockedId)
    {
        return await _context.BlockedUsers
            .FirstOrDefaultAsync(b => b.BlockerId == blockerId && b.BlockedId == blockedId);
    }

    public async Task<List<BlockedUser>> GetByBlockerIdAsync(int blockerId)
    {
        return await _context.BlockedUsers
            .Where(b => b.BlockerId == blockerId)
            .Include(b => b.Blocked)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync();
    }

    public async Task<HashSet<int>> GetBlockedUserIdsForViewerAsync(int viewerId)
    {
        var ids = await _context.BlockedUsers
            .Where(b => b.BlockerId == viewerId || b.BlockedId == viewerId)
            .Select(b => b.BlockerId == viewerId ? b.BlockedId : b.BlockerId)
            .ToListAsync();

        return ids.ToHashSet();
    }

    public async Task AddAsync(BlockedUser block)
    {
        await _context.BlockedUsers.AddAsync(block);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(BlockedUser block)
    {
        _context.BlockedUsers.Remove(block);
        await _context.SaveChangesAsync();
    }
}
