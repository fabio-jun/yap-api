using System.Security.Claims;
using Blog.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Blog.API.Controllers;

[ApiController]
[Route("api/posts/{postId}/likes")]
public class LikesController : ControllerBase
{
    private readonly ILikeService _likeService;

    public LikesController(ILikeService likeService)
    {
        _likeService = likeService;
    }

    // POST api/posts/{postId}/likes
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Toggle(int postId)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var liked = await _likeService.ToggleLikeAsync(postId, userId);
        var count = await _likeService.GetCountAsync(postId);

        return Ok(new { liked, count });
    }

    // GET api/posts/{postId}/likes — get like count and whether current user liked
    [HttpGet]
    public async Task<IActionResult> GetLikes(int postId)
    {
        var count = await _likeService.GetCountAsync(postId);

        // If the user is authenticated, check if they liked the post
        bool liked = false;
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim != null)
        {
            liked = await _likeService.HasLikedAsync(postId, int.Parse(userIdClaim.Value));
        }

        return Ok(new { liked, count });
    }
}