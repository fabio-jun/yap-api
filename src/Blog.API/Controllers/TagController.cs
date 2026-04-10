using Blog.Application.DTOs.Tags;
using Blog.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Blog.API.Controllers;

// API controller for tag (hashtag) operations.
// Tags are auto-extracted from post content (e.g., #dev, #react).
// This controller provides read-only access to the tag list.
[ApiController]
[Route("api/[controller]")]
public class TagController : ControllerBase
{
    private readonly ITagService _tagService;

    public TagController(ITagService tagService)
    {
        _tagService = tagService;
    }

    // GET api/tag — public, returns all tags ordered by popularity (most posts first).
    // Used by the frontend to display trending/popular hashtags.
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var tags = await _tagService.GetAllAsync();
        return Ok(tags);
    }
}
