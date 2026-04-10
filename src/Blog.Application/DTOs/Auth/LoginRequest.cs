namespace Blog.Application.DTOs.Auth;

// DTO for login — client sends email + password, server validates and returns tokens.
public class LoginRequest
{
    public required string Email { get; set; }
    public required string Password { get; set; }
}
