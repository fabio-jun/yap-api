namespace Blog.Application.DTOs.Auth;

// DTO returned to the client after successful login or registration.
// Contains the JWT access token (for API calls) and a refresh token (for getting new access tokens).
// ASP.NET Core serializes this object into JSON automatically when returned via Ok(response).
public class AuthResponse
{
    // JWT token string — sent in the Authorization header as "Bearer <token>"
    public required string AccessToken { get; set; }

    // Random GUID string — stored in DB, sent to /api/auth/refresh to get a new access token
    public required string RefreshToken { get; set; }

    // When the refresh token expires (access token expires in 1 hour, refresh in 7 days)
    public required DateTime ExpiresAt { get; set; }
}
