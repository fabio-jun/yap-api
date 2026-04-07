//Simple class with properties that represent the data the client sends/receives

namespace Blog.Application.DTOs.Auth;

public class RegisterRequest
{
    public required string UserName { get; set; }
    public required string Email { get; set; }
    public required string Password { get; set; }

}