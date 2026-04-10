namespace Blog.Domain.Entities;

// Entity class — represents the "RefreshTokens" table in the database.
// Refresh tokens allow the client to get a new JWT access token without re-entering credentials.
// Flow: client sends expired access token + valid refresh token → server issues new pair.
public class RefreshToken
{
    // Primary Key — auto-incremented
    public int Id { get; set; }

    // The token string itself — a random GUID stored as text. Sent to the client.
    public required string Token { get; set; }

    // When this refresh token expires (typically 7 days from creation)
    public DateTime ExpiresAt { get; set; }

    // Marked true when the token is used or manually revoked. Prevents reuse.
    public bool IsRevoked { get; set; }

    // FK — which user this token belongs to
    public int UserId { get; set; }
    public User? User { get; set; }
}
