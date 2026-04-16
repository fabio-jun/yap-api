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

    // FirstOrDefaultAsync — returns the first block relationship matching both users, or null.
    // Used by the service layer to check if a user already blocked another user.
    public async Task<BlockedUser?> GetAsync(int blockerId, int blockedId)
    {
        return await _context.BlockedUsers
            .FirstOrDefaultAsync(b => b.BlockerId == blockerId && b.BlockedId == blockedId);
    }

    // Returns all users blocked by a specific user.
    // Include(b => b.Blocked) — eager loads the blocked user's data for display in the UI.
    // OrderByDescending — most recent blocks first (SQL: ORDER BY "CreatedAt" DESC).
    public async Task<List<BlockedUser>> GetByBlockerIdAsync(int blockerId)
    {
        return await _context.BlockedUsers
            .Where(b => b.BlockerId == blockerId)
            .Include(b => b.Blocked)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync();
    }

    // Returns user IDs that should be hidden from the viewer.
    // Where() checks both directions: users blocked by the viewer and users who blocked the viewer.
    // Select() projects each BlockedUser row to the other user's ID.
    // ToHashSet() removes duplicates and gives fast lookup with Contains().
    public async Task<HashSet<int>> GetBlockedUserIdsForViewerAsync(int viewerId)
    {
        var ids = await _context.BlockedUsers
            .Where(b => b.BlockerId == viewerId || b.BlockedId == viewerId)
            .Select(b => b.BlockerId == viewerId ? b.BlockedId : b.BlockerId)
            .ToListAsync();

        return ids.ToHashSet();
    }

    // AddAsync() stages the block relationship for INSERT in the Change Tracker.
    // SaveChangesAsync() flushes the INSERT to the database.
    public async Task AddAsync(BlockedUser block)
    {
        await _context.BlockedUsers.AddAsync(block);
        await _context.SaveChangesAsync();
    }

    // Remove() marks the block relationship for DELETE in the Change Tracker.
    // The actual DELETE SQL executes on SaveChangesAsync().
    public async Task DeleteAsync(BlockedUser block)
    {
        _context.BlockedUsers.Remove(block);
        await _context.SaveChangesAsync();
    }
}
