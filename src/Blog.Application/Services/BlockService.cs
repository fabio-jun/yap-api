using Blog.Application.DTOs.Blocks;
using Blog.Application.Interfaces;
using Blog.Domain.Entities;
using Blog.Domain.Interfaces;

namespace Blog.Application.Services;

public class BlockService : IBlockService
{
    private readonly IBlockRepository _blockRepository;
    private readonly IUserRepository _userRepository;

    public BlockService(IBlockRepository blockRepository, IUserRepository userRepository)
    {
        _blockRepository = blockRepository;
        _userRepository = userRepository;
    }

    public async Task BlockAsync(int blockerId, int blockedId)
    {
        if (blockerId == blockedId)
            throw new ArgumentException("You cannot block yourself.");

        var blockedUser = await _userRepository.GetByIdAsync(blockedId);
        if (blockedUser == null)
            throw new KeyNotFoundException("User not found.");

        var existing = await _blockRepository.GetAsync(blockerId, blockedId);
        if (existing != null) return;

        await _blockRepository.AddAsync(new BlockedUser
        {
            BlockerId = blockerId,
            BlockedId = blockedId,
            CreatedAt = DateTime.UtcNow
        });
    }

    public async Task UnblockAsync(int blockerId, int blockedId)
    {
        var existing = await _blockRepository.GetAsync(blockerId, blockedId);
        if (existing != null)
        {
            await _blockRepository.DeleteAsync(existing);
        }
    }

    public async Task<bool> IsBlockedAsync(int blockerId, int blockedId)
    {
        return await _blockRepository.GetAsync(blockerId, blockedId) != null;
    }

    public async Task<List<BlockedUserResponse>> GetBlockedUsersAsync(int blockerId)
    {
        var blocks = await _blockRepository.GetByBlockerIdAsync(blockerId);
        return blocks.Select(b => new BlockedUserResponse
        {
            UserId = b.BlockedId,
            UserName = b.Blocked?.UserName ?? string.Empty,
            ProfileImageUrl = b.Blocked?.ProfileImageUrl,
            CreatedAt = b.CreatedAt
        }).ToList();
    }
}
