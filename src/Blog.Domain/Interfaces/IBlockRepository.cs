using Blog.Domain.Entities;

namespace Blog.Domain.Interfaces;

public interface IBlockRepository
{
    // Searches for a specific block
    Task<BlockedUser?> GetAsync(int blockerId, int blockedId);

    // Searches for all the blocks made by a user
    Task<List<BlockedUser>> GetByBlockerIdAsync(int blockerId);

    // Searches for all the blocked users that should be hidden from a viewer
    Task<HashSet<int>> GetBlockedUserIdsForViewerAsync(int viewerId);
    
    Task AddAsync(BlockedUser block);
    Task DeleteAsync(BlockedUser block);
}
