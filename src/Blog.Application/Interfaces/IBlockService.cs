using Blog.Application.DTOs.Blocks;

namespace Blog.Application.Interfaces;

public interface IBlockService
{
    Task BlockAsync(int blockerId, int blockedId);
    Task UnblockAsync(int blockerId, int blockedId);
    Task<bool> IsBlockedAsync(int blockerId, int blockedId);
    Task<List<BlockedUserResponse>> GetBlockedUsersAsync(int blockerId);
}
