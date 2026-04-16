using Blog.Application.DTOs.Blocks;
using Blog.Application.Interfaces;
using Blog.Domain.Entities;
using Blog.Domain.Interfaces;

namespace Blog.Application.Services;

public class BlockService : IBlockService
{
    private readonly IBlockRepository _blockRepository;
    private readonly IUserRepository _userRepository;

    // Repositories are injected by ASP.NET Core's DI container.
    public BlockService(IBlockRepository blockRepository, IUserRepository userRepository)
    {
        _blockRepository = blockRepository;
        _userRepository = userRepository;
    }

    // Blocks another user
    public async Task BlockAsync(int blockerId, int blockedId)
    {
        // Business rule: users cannot block themselves.
        if (blockerId == blockedId)
            throw new ArgumentException("You cannot block yourself.");

        // Validate the target before creating the block relationship.
        var blockedUser = await _userRepository.GetByIdAsync(blockedId);
        if (blockedUser == null)
            throw new KeyNotFoundException("User not found.");

        // Avoid duplicate block rows for the same blocker/blocked pair.
        var existing = await _blockRepository.GetAsync(blockerId, blockedId);
        if (existing != null) return;

        await _blockRepository.AddAsync(new BlockedUser
        {
            BlockerId = blockerId,
            BlockedId = blockedId,
            CreatedAt = DateTime.UtcNow
        });
    }

    // Removes an existing block. Missing rows are treated as already unblocked.
    public async Task UnblockAsync(int blockerId, int blockedId)
    {
        var existing = await _blockRepository.GetAsync(blockerId, blockedId);
        if (existing != null)
        {
            await _blockRepository.DeleteAsync(existing);
        }
    }

    // Checks whether blockerId has blocked blockedId.
    public async Task<bool> IsBlockedAsync(int blockerId, int blockedId)
    {
        return await _blockRepository.GetAsync(blockerId, blockedId) != null;
    }

    // Returns users blocked by the authenticated account, mapped to public DTOs.
    public async Task<List<BlockedUserResponse>> GetBlockedUsersAsync(int blockerId)
    {
        var blocks = await _blockRepository.GetByBlockerIdAsync(blockerId);
        // Blocked navigation can be null if the repository did not include user details.
        return blocks.Select(b => new BlockedUserResponse
        {
            UserId = b.BlockedId,
            UserName = b.Blocked?.UserName ?? string.Empty,
            ProfileImageUrl = b.Blocked?.ProfileImageUrl,
            CreatedAt = b.CreatedAt
        }).ToList();
    }
}
