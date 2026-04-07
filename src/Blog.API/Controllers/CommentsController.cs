using System.Security.Claims;
using Blog.Application.DTOs.Comments;
using Blog.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Blog.API.Controllers;

// Attribute that enables API behaviors
[ApiController]
// Base route: api/posts/{postId}/comments (nested under posts)
[Route("api/posts/{postId}/comments")]
public class CommentsController : ControllerBase
{
    // External dependency used for dependency injection
    private readonly ICommentService _commentService;

    // Constructor: DI
    public CommentsController(ICommentService commentService)
    {
        _commentService = commentService;
    }

    // GET api/posts/{postId}/comments — public, returns all comments for a post
    [HttpGet]
    public async Task<IActionResult> GetByPostId(int postId)
    {
        var comments = await _commentService.GetByPostIdAsync(postId);
        // Returns HTTP 200 (OK) with comments serialized as JSON
        return Ok(comments);
    }

    // POST api/posts/{postId}/comments — authenticated, creates a new comment
    [HttpPost]
    [Authorize] // Protects the route with JWT
    public async Task<IActionResult> Create(int postId, CreateCommentRequest request)
    {
        // Extracts the user ID from the JWT token claims
        var authorId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var comment = await _commentService.CreateAsync(postId, request, authorId);
        return Ok(comment);
    }

    // DELETE api/posts/{postId}/comments/{id} — authenticated, author or admin can delete
    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> Delete(int postId, int id)
    {
        // Extracts user ID and role from JWT to check authorization in the service
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var userRole = User.FindFirst(ClaimTypes.Role)!.Value;

        await _commentService.DeleteAsync(id, userId, userRole);
        // Returns HTTP 204 (No Content) — standard for successful DELETE
        return NoContent();
    }
}
