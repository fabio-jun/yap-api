using System.Security.Claims;
using Blog.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Blog.API.Controllers;

// API controller for the user's personalized feed.
// The feed shows posts only from users the authenticated user follows.
// Separate from PostController to keep concerns clean (global posts vs. personal feed).
[ApiController]
[Route("api/[controller]")]
public class FeedController : ControllerBase
{
    private readonly IPostService _postService;

    // Expression-bodied constructor.
    public FeedController(IPostService postService) => _postService = postService;

    // GET api/feed — returns posts from followed users, ordered by most recent.
    // [Authorize] — requires a valid JWT (the feed is inherently user-specific).
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetFeed()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var posts = await _postService.GetFeedAsync(userId);
        return Ok(posts);
    }
}
