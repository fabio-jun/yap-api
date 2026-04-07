using Blog.Application.DTOs.Posts;
using Blog.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Blog.API.Controllers;

// Attribute that enables API behaviors
[ApiController]
// Defines controller's base route
[Route("api/[controller]")]
public class PostController : ControllerBase
{
    // External dependency used for dependency injection
    private readonly IPostService _postService;

    // Constructor: DI
    public PostController(IPostService postService)
    {
        _postService = postService;
    }

    // GET api/post — public, returns all posts with like info for the current user
    // Supports ?search=texto, ?tag=csharp, ?page=1&pageSize=20 query params
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? search,
        [FromQuery] string? tag,
        [FromQuery] int? page,
        [FromQuery] int? pageSize)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        int? currentUserId = userIdClaim != null ? int.Parse(userIdClaim.Value) : null;

        if (!string.IsNullOrWhiteSpace(search))
        {
            var results = await _postService.SearchAsync(search, currentUserId);
            return Ok(results);
        }

        if (!string.IsNullOrWhiteSpace(tag))
        {
            var results = await _postService.GetByTagAsync(tag, currentUserId);
            return Ok(results);
        }

        if (page.HasValue)
        {
            var paged = await _postService.GetAllPagedAsync(page.Value, pageSize ?? 20, currentUserId);
            return Ok(paged);
        }

        var posts = await _postService.GetAllAsync(currentUserId);
        return Ok(posts);
    }

    // GET api/post/{id} — public, returns a single post with like info for the current user
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        int? currentUserId = userIdClaim != null ? int.Parse(userIdClaim.Value) : null;

        var post = await _postService.GetByIdAsync(id, currentUserId);
        return Ok(post);
    }

    // POST api/post — authenticated, creates a new post
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create(CreatePostRequest request)
    {
        // Extracts the user ID from the JWT token claims
        var authorId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var post = await _postService.CreateAsync(request, authorId);
        return Ok(post);
    }

    // PUT api/post/{id} — authenticated, author or admin can update
    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> Update(int id, UpdatePostRequest request)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var userRole = User.FindFirst(ClaimTypes.Role)!.Value;

        var post = await _postService.UpdateAsync(id, request, userId, userRole);
        return Ok(post);
    }

    // DELETE api/post/{id} — authenticated, author or admin can delete
    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var userRole = User.FindFirst(ClaimTypes.Role)!.Value;

        await _postService.DeleteAsync(id, userId, userRole);
        // Returns HTTP 204 (No Content) — standard for successful DELETE
        return NoContent();
    }
}
