namespace Blog.Application.DTOs.Auth;

// DTO (Data Transfer Object) — a simple class with properties that represent
// the data the client sends in the HTTP request body.
// ASP.NET Core automatically deserializes the JSON body into this object (model binding).
// Example JSON: { "userName": "fabio", "email": "fabio@x.com", "password": "123456" }
public class RegisterRequest
{
    // 'required' keyword ensures these fields must be present in the JSON body.
    // If missing, ASP.NET returns 400 Bad Request automatically.
    public required string UserName { get; set; }
    public required string Email { get; set; }
    public required string Password { get; set; }
}
