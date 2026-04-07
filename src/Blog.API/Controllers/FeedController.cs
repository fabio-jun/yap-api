using System.Security.Claims;
using Blog.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Blog.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FeedController : ControllerBase
{
    private readonly IPostService _postService;

    public FeedController(IPostService postService) => _postService = postService;

    // GET api/feed — returns posts from users the authenticated user follows
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetFeed()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var posts = await _postService.GetFeedAsync(userId);
        return Ok(posts);
    }
}
