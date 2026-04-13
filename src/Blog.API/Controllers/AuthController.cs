using Blog.Application.DTOs.Auth;
using Blog.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Blog.API.Controllers;

// Attribute that you put on a controller class to indicate that it responds to web API requests. It enables features like automatic model validation
[ApiController]
// Attribute routing, [controller] is replaced by the class name.
// Handles: api/auth/register, api/auth/login, api/auth/refresh
[Route("api/[controller]")]
// ControllerBase — base class for API controllers.
// Provides helper methods: Ok(), BadRequest(), NotFound(), NoContent(), etc.
public class AuthController : ControllerBase
{
    // Dependency injected via constructor — the DI container resolves IAuthService to AuthService.
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    // [HttpPost("register")] — maps POST requests to api/auth/register.
    // RegisterRequest parameter is automatically deserialized from the JSON request body
    // (ApiController infers [FromBody] for complex types).
    // Returns Task<IActionResult> — async method returning an HTTP response.
    [HttpPost("register")]
    [SwaggerOperation(Summary = "Register a new user", Description = "Creates a user account and returns an access token, refresh token, and user profile data.")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        var response = await _authService.RegisterAsync(request);
        // Ok(response) — returns HTTP 200 with the AuthResponse (tokens + user info) as JSON.
        return Ok(response);
    }

    // POST api/auth/login — authenticates a user and returns JWT + refresh token.
    [HttpPost("login")]
    [SwaggerOperation(Summary = "Log in", Description = "Authenticates a user by email and password and returns a new token pair.")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var response = await _authService.LoginAsync(request);
        return Ok(response);
    }

    // POST api/auth/refresh — exchanges a valid refresh token for a new token pair.
    // [FromBody] string — binds the raw request body string (not a JSON object).
    // This is needed because ASP.NET Core doesn't auto-bind primitive types from the body.
    [HttpPost("refresh")]
    [SwaggerOperation(Summary = "Refresh tokens", Description = "Exchanges a valid refresh token for a new access token and refresh token.")]
    public async Task<IActionResult> Refresh([FromBody] string refreshToken)
    {
        var response = await _authService.RefreshTokenAsync(refreshToken);
        return Ok(response);
    }
}
