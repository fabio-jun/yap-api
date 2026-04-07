using System.Security.Claims;
using Blog.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Blog.API.Controllers;

[ApiController]
[Route("api/users/{userId}")]
public class FollowsController : ControllerBase
{
    private readonly IFollowService _followService;

    public FollowsController(IFollowService followService) => _followService = followService;

    // POST api/users/{userId}/follow — toggle follow/unfollow
    [HttpPost("follow")]
    [Authorize]
    public async Task<IActionResult> Toggle(int userId)
    {
        var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var following = await _followService.ToggleFollowAsync(currentUserId, userId);
        return Ok(new { following });
    }

    // GET api/users/{userId}/followers
    [HttpGet("followers")]
    public async Task<IActionResult> GetFollowers(int userId)
    {
        var followers = await _followService.GetFollowersAsync(userId);
        return Ok(followers);
    }

    // GET api/users/{userId}/following
    [HttpGet("following")]
    public async Task<IActionResult> GetFollowing(int userId)
    {
        var following = await _followService.GetFollowingAsync(userId);
        return Ok(following);
    }
}
