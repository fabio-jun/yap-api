using Blog.Application.DTOs.Auth;
using Blog.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Blog.API.Controllers;

//Attribute that enables API behaviors
[ApiController]
//Defines controller's base route 
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    //External dependencies used for dependency Injection
    private readonly IAuthService _authService;

    // Constructor: DI
    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    //Attribute that defines the following method answers to HTTP POST at route api/auth/register
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        //Calls the RegisterAsync method from the service
        var response = await _authService.RegisterAsync(request);
        //Returns HTTP 200 (OK) with an response object serialized as JSON in the body
        return Ok(response);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var response = await _authService.LoginAsync(request);
        return Ok(response);
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] string refreshToken)
    {
        var response = await _authService.RefreshTokenAsync(refreshToken);
        return Ok(response);
    }



}