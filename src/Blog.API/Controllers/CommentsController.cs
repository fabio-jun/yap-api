using System.Security.Claims;
using Blog.Application.DTOs.Comments;
using Blog.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Blog.API.Controllers;

// API controller for comment CRUD operations.
// Uses nested routing: comments are scoped under a specific post.
[ApiController]
// Nested route — comments are accessed as sub-resources of posts.
// {postId} is a route parameter captured from the URL and bound to method parameters.
// Example: GET api/posts/42/comments, POST api/posts/42/comments
[Route("api/posts/{postId}/comments")]
public class CommentsController : ControllerBase
{
    private readonly ICommentService _commentService;

    public CommentsController(ICommentService commentService)
    {
        _commentService = commentService;
    }

    // GET api/posts/{postId}/comments — public, returns all comments for a post.
    [HttpGet]
    public async Task<IActionResult> GetByPostId(int postId)
    {
        var comments = await _commentService.GetByPostIdAsync(postId);
        return Ok(comments);
    }

    // POST api/posts/{postId}/comments — authenticated, creates a new comment on the post.
    // [Authorize] — middleware validates the JWT Bearer token before this method executes.
    // If invalid/missing, ASP.NET Core returns 401 Unauthorized automatically.
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create(int postId, CreateCommentRequest request)
    {
        // Extracts the user ID from the JWT claims. Safe to use '!' because [Authorize] guarantees it.
        var authorId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var comment = await _commentService.CreateAsync(postId, request, authorId);
        return Ok(comment);
    }

    // DELETE api/posts/{postId}/comments/{id} — deletes a comment by ID.
    // {id} is an additional route segment appended to the controller's base route.
    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> Delete(int postId, int id)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var userRole = User.FindFirst(ClaimTypes.Role)!.Value;

        await _commentService.DeleteAsync(id, userId, userRole);
        return NoContent();
    }
}
