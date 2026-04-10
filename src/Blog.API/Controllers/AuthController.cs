using Blog.Application.DTOs.Auth;
using Blog.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Blog.API.Controllers;

// [ApiController] — enables automatic model validation, binding source inference,
// and structured error responses (returns 400 automatically for invalid models).
[ApiController]
// [Route("api/[controller]")] — attribute routing. [controller] is replaced by the class name minus "Controller".
// This controller handles: api/auth/register, api/auth/login, api/auth/refresh
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    // ControllerBase — base class for API controllers (no View support, unlike Controller).
    // Provides helper methods: Ok(), BadRequest(), NotFound(), NoContent(), etc.

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
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        var response = await _authService.RegisterAsync(request);
        // Ok(response) — returns HTTP 200 with the AuthResponse (tokens + user info) as JSON.
        return Ok(response);
    }

    // POST api/auth/login — authenticates a user and returns JWT + refresh token.
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var response = await _authService.LoginAsync(request);
        return Ok(response);
    }

    // POST api/auth/refresh — exchanges a valid refresh token for a new token pair.
    // [FromBody] string — explicitly binds the raw request body string (not a JSON object).
    // This is needed because ASP.NET Core doesn't auto-bind primitive types from the body.
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] string refreshToken)
    {
        var response = await _authService.RefreshTokenAsync(refreshToken);
        return Ok(response);
    }
}
