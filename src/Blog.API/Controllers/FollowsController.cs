using System.Security.Claims;
using Blog.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Blog.API.Controllers;

// API controller for follow/unfollow operations.
// Route is nested under users: api/users/{userId}/follow, followers, following
// Uses the toggle pattern — POST toggles follow on/off.
[ApiController]
[Route("api/users/{userId}")]
public class FollowsController : ControllerBase
{
    private readonly IFollowService _followService;

    // Expression-bodied constructor — shorthand for single-line constructors.
    public FollowsController(IFollowService followService) => _followService = followService;

    // POST api/users/{userId}/follow — toggles follow/unfollow.
    // {userId} from the route is the target user to follow/unfollow.
    // The current user's ID comes from the JWT token.
    [HttpPost("follow")]
    [Authorize]
    [SwaggerOperation(Summary = "Toggle follow", Description = "Follows or unfollows the target user for the authenticated account.")]
    public async Task<IActionResult> Toggle(int userId)
    {
        var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var following = await _followService.ToggleFollowAsync(currentUserId, userId);
        // Returns { "following": true } or { "following": false }
        return Ok(new { following });
    }

    // GET api/users/{userId}/followers — returns the list of users who follow this user.
    [HttpGet("followers")]
    [SwaggerOperation(Summary = "Get followers", Description = "Returns users who follow the target user.")]
    public async Task<IActionResult> GetFollowers(int userId)
    {
        var followers = await _followService.GetFollowersAsync(userId);
        return Ok(followers);
    }

    // GET api/users/{userId}/following — returns the list of users this user follows.
    [HttpGet("following")]
    [SwaggerOperation(Summary = "Get following", Description = "Returns users followed by the target user.")]
    public async Task<IActionResult> GetFollowing(int userId)
    {
        var following = await _followService.GetFollowingAsync(userId);
        return Ok(following);
    }
}
