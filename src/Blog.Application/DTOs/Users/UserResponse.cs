namespace Blog.Application.DTOs.Users;

// DTO for returning user profile data to the client.
// Includes follow counts (computed from the Follow table) that don't exist on the User entity.
// Never exposes PasswordHash — DTOs act as a security boundary.
public class UserResponse
{
    public int Id { get; set; }
    public required string UserName { get; set; }
    public required string Email { get; set; }
    public required string Role { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? ProfileImageUrl { get; set; }
    public string? DisplayName { get; set; }
    public string? Bio { get; set; }

    // Computed fields — not stored on the User entity, calculated via COUNT queries
    public int FollowersCount { get; set; }
    public int FollowingCount { get; set; }
}
