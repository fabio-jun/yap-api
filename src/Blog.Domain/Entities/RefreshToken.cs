namespace Blog.Domain.Entities;

public class RefreshToken
{
    // PK
    public int Id { get; set; }
    public required string Token { get; set; }
    public DateTime ExpiresAt { get; set; }
    public bool IsRevoked { get; set; }

    // FK — which user this token belongs to
    public int UserId { get; set; }
    public User? User { get; set; }
}
