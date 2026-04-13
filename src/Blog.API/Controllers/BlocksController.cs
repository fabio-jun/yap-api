using System.Security.Claims;
using Blog.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Blog.API.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]
public class BlocksController : ControllerBase
{
    private readonly IBlockService _blockService;

    public BlocksController(IBlockService blockService)
    {
        _blockService = blockService;
    }

    [HttpPost("{userId}/block")]
    [SwaggerOperation(Summary = "Block user", Description = "Blocks a user for the authenticated account. Blocked users are hidden from feed results.")]
    public async Task<IActionResult> Block(int userId)
    {
        var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        await _blockService.BlockAsync(currentUserId, userId);
        return NoContent();
    }

    [HttpDelete("{userId}/block")]
    [SwaggerOperation(Summary = "Unblock user", Description = "Removes a user block for the authenticated account.")]
    public async Task<IActionResult> Unblock(int userId)
    {
        var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        await _blockService.UnblockAsync(currentUserId, userId);
        return NoContent();
    }

    [HttpGet("{userId}/block")]
    [SwaggerOperation(Summary = "Get block state", Description = "Returns whether the authenticated user has blocked the target user.")]
    public async Task<IActionResult> IsBlocked(int userId)
    {
        var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var blocked = await _blockService.IsBlockedAsync(currentUserId, userId);
        return Ok(new { blocked });
    }

    [HttpGet("blocked")]
    [SwaggerOperation(Summary = "Get blocked users", Description = "Returns users blocked by the authenticated account.")]
    public async Task<IActionResult> GetBlockedUsers()
    {
        var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var users = await _blockService.GetBlockedUsersAsync(currentUserId);
        return Ok(users);
    }
}
