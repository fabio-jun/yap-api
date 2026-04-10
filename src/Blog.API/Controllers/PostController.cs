using Blog.Application.DTOs.Posts;
using Blog.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Blog.API.Controllers;

// API controller for post (yap) CRUD operations.
// Handles listing, searching, filtering by tag, pagination, and CRUD with authorization.
[ApiController]
[Route("api/[controller]")]
public class PostController : ControllerBase
{
    private readonly IPostService _postService;

    public PostController(IPostService postService)
    {
        _postService = postService;
    }

    // GET api/post — public endpoint with multiple optional query parameters.
    // [FromQuery] — binds parameters from the URL query string (?search=text&tag=dev&page=1).
    // Nullable types (string?, int?) allow the parameters to be optional.
    // The method dispatches to different service methods based on which params are present.
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? search,
        [FromQuery] string? tag,
        [FromQuery] int? page,
        [FromQuery] int? pageSize)
    {
        // User.FindFirst() — reads claims from the JWT token (if present).
        // ClaimTypes.NameIdentifier — standard claim type for user ID.
        // Works for both authenticated and anonymous users (returns null if no token).
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        // Ternary operator + nullable int: extracts userId from claim, or null if anonymous.
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

        // HasValue — checks if a nullable type has a value (int? can be null or an int).
        // .Value — unwraps the nullable to get the underlying int.
        // ?? — null-coalescing: uses 20 as default page size if not provided.
        if (page.HasValue)
        {
            var paged = await _postService.GetAllPagedAsync(page.Value, pageSize ?? 20, currentUserId);
            return Ok(paged);
        }

        var posts = await _postService.GetAllAsync(currentUserId);
        return Ok(posts);
    }

    // GET api/post/user/{userId} — route parameter {userId} is bound to the method parameter.
    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetByUserId(int userId)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        int? currentUserId = userIdClaim != null ? int.Parse(userIdClaim.Value) : null;

        var posts = await _postService.GetByUserIdAsync(userId, currentUserId);
        return Ok(posts);
    }

    // GET api/post/{id} — returns a single post by ID.
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        int? currentUserId = userIdClaim != null ? int.Parse(userIdClaim.Value) : null;

        var post = await _postService.GetByIdAsync(id, currentUserId);
        return Ok(post);
    }

    // POST api/post — creates a new post. Requires authentication.
    // [Authorize] — rejects unauthenticated requests with 401. The JWT must be valid.
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create(CreatePostRequest request)
    {
        // User.FindFirst(ClaimTypes.NameIdentifier)! — the '!' (null-forgiving) is safe here
        // because [Authorize] guarantees a valid JWT with claims exists.
        var authorId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var post = await _postService.CreateAsync(request, authorId);
        return Ok(post);
    }

    // PUT api/post/{id} — updates a post. Service layer checks ownership (author or admin).
    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> Update(int id, UpdatePostRequest request)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        // ClaimTypes.Role — reads the "role" claim from the JWT (e.g., "User" or "Admin").
        var userRole = User.FindFirst(ClaimTypes.Role)!.Value;

        var post = await _postService.UpdateAsync(id, request, userId, userRole);
        return Ok(post);
    }

    // DELETE api/post/{id} — deletes a post. Service layer checks ownership.
    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var userRole = User.FindFirst(ClaimTypes.Role)!.Value;

        await _postService.DeleteAsync(id, userId, userRole);
        // NoContent() — returns HTTP 204. Standard REST response for successful DELETE (no body).
        return NoContent();
    }
}
