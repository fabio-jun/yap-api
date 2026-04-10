using System.Security.Claims;
using Blog.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Blog.API.Controllers;

// API controller for like operations.
// Nested under posts: api/posts/{postId}/likes
// Uses the toggle pattern — POST toggles like on/off (no separate unlike endpoint).
[ApiController]
[Route("api/posts/{postId}/likes")]
public class LikesController : ControllerBase
{
    private readonly ILikeService _likeService;

    public LikesController(ILikeService likeService)
    {
        _likeService = likeService;
    }

    // POST api/posts/{postId}/likes — toggles like for the authenticated user.
    // Returns both the new state (liked: true/false) and the updated count.
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Toggle(int postId)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        // ToggleLikeAsync returns bool — true if liked, false if unliked.
        var liked = await _likeService.ToggleLikeAsync(postId, userId);
        var count = await _likeService.GetCountAsync(postId);

        // Anonymous object — serialized to JSON: { "liked": true, "count": 42 }
        // Property names become the JSON keys.
        return Ok(new { liked, count });
    }

    // GET api/posts/{postId}/likes — returns like count and whether the current user liked.
    // Public endpoint — works for both authenticated and anonymous users.
    [HttpGet]
    public async Task<IActionResult> GetLikes(int postId)
    {
        var count = await _likeService.GetCountAsync(postId);

        // For anonymous users, liked defaults to false.
        bool liked = false;
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim != null)
        {
            liked = await _likeService.HasLikedAsync(postId, int.Parse(userIdClaim.Value));
        }

        return Ok(new { liked, count });
    }
}
