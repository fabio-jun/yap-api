using System.Security.Claims;
using Blog.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Blog.API.Controllers;

// API controller for bookmark operations.
// [Authorize] at class level — ALL endpoints require authentication (no public access).
// Uses the toggle pattern — POST toggles bookmark on/off.
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
    public async Task<IActionResult> Toggle(int postId)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var bookmarked = await _bookmarkService.ToggleBookmarkAsync(postId, userId);
        return Ok(new { bookmarked });
    }

    // GET api/bookmarks — returns all bookmarked posts for the authenticated user.
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var posts = await _bookmarkService.GetBookmarksAsync(userId);
        return Ok(posts);
    }
}
