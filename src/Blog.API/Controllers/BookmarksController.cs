using System.Security.Claims;
using Blog.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Blog.API.Controllers;

// API controller for bookmark operations
// [Authorize] at class level — All endpoints require authentication, so no public access.
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BookmarksController : ControllerBase
{
    private readonly IBookmarkService _bookmarkService;

    public BookmarksController(IBookmarkService bookmarkService)
    {
        _bookmarkService = bookmarkService;
    }

    // POST api/bookmarks/{postId} — toggles bookmark for the given post.
    // Returns { "bookmarked": true } or { "bookmarked": false }.
    [HttpPost("{postId}")]
    [SwaggerOperation(Summary = "Toggle bookmark", Description = "Adds or removes a yap bookmark for the authenticated user.")]
    public async Task<IActionResult> Toggle(int postId)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var bookmarked = await _bookmarkService.ToggleBookmarkAsync(postId, userId);
        return Ok(new { bookmarked });
    }

    // GET api/bookmarks — returns all bookmarked posts for the authenticated user.
    [HttpGet]
    [SwaggerOperation(Summary = "Get bookmarks", Description = "Returns yaps bookmarked by the authenticated user.")]
    public async Task<IActionResult> GetAll()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var posts = await _bookmarkService.GetBookmarksAsync(userId);
        return Ok(posts);
    }
}
