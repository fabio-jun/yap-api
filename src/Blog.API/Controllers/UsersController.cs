using System.Security.Claims;
using Blog.Application.DTOs.Users;
using Blog.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Blog.API.Controllers;

// API controller for user profile operations.
// Handles search, profile viewing, suggested users, and profile editing.
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    // Expression-bodied constructor — shorthand for single-statement constructors.
    // Equivalent to: public UsersController(IUserService userService) { _userService = userService; }
    public UsersController(IUserService userService) => _userService = userService;

    // GET api/users?search=nome — searches users by name (query string parameter).
    // [FromQuery] — explicitly binds from the URL query string.
    [HttpGet]
    public async Task<IActionResult> Search([FromQuery] string? search)
    {
        if (string.IsNullOrWhiteSpace(search))
            // BadRequest() — returns HTTP 400 with an error message.
            return BadRequest("Search query is required.");

        var users = await _userService.SearchAsync(search);
        return Ok(users);
    }

    // GET api/users/{id} — public, returns a user's profile.
    // {id:int} — route constraint: only matches if {id} is a valid integer.
    // This prevents ambiguity with other string-based routes (like "suggested").
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var user = await _userService.GetByIdAsync(id);
        return Ok(user);
    }

    // GET api/users/suggested — returns users the current user doesn't follow.
    // Requires authentication to know which user is requesting suggestions.
    [HttpGet("suggested")]
    [Authorize]
    public async Task<IActionResult> GetSuggested()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        // 5 — hardcoded limit for suggested users (returns at most 5 suggestions).
        var users = await _userService.GetSuggestedAsync(userId, 5);
        return Ok(users);
    }

    // PUT api/users/me — updates the authenticated user's own profile.
    // "me" is a literal route segment (not a parameter) — RESTful convention for "current user".
    [HttpPut("me")]
    [Authorize]
    public async Task<IActionResult> UpdateMe(UpdateUserRequest request)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var user = await _userService.UpdateAsync(userId, request);
        return Ok(user);
    }
}
