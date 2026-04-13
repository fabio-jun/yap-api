using System.Security.Claims;
using Blog.Application.DTOs.Reposts;
using Blog.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Blog.API.Controllers;

[ApiController]
[Route("api/posts/{postId}")]
public class RepostsController : ControllerBase
{
    private readonly IRepostService _repostService;

    public RepostsController(IRepostService repostService)
    {
        _repostService = repostService;
    }

    [HttpPost("reposts")]
    [Authorize]
    [SwaggerOperation(Summary = "Toggle re-yap", Description = "Creates or removes a simple re-yap for the authenticated user.")]
    public async Task<IActionResult> Toggle(int postId)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var result = await _repostService.ToggleAsync(postId, userId);
        return Ok(result);
    }

    [HttpPost("quote-reposts")]
    [Authorize]
    [SwaggerOperation(Summary = "Quote re-yap", Description = "Creates or updates a quote re-yap with the authenticated user's comment.")]
    public async Task<IActionResult> Quote(int postId, QuoteRepostRequest request)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var result = await _repostService.QuoteAsync(postId, request, userId);
        return Ok(result);
    }

    [HttpGet("reposts")]
    [SwaggerOperation(Summary = "Get re-yap state", Description = "Returns whether the current viewer re-yapped the yap, the repost count, and quote content when available.")]
    public async Task<IActionResult> GetState(int postId)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        var userId = userIdClaim != null ? int.Parse(userIdClaim.Value) : (int?)null;
        var result = await _repostService.GetStateAsync(postId, userId);
        return Ok(result);
    }
}
