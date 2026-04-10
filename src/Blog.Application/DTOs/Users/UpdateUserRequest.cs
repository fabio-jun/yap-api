namespace Blog.Application.DTOs.Users;

// DTO for updating the authenticated user's profile.
// Only the user themselves can update their profile (enforced by the controller extracting userId from JWT).
public class UpdateUserRequest
{
    public required string userName { get; set; }
    public required string Email { get; set; }

    // Optional fields — null means "don't change" or "clear the value"
    public string? ProfileImageUrl { get; set; }
    public string? DisplayName { get; set; }
    public string? Bio { get; set; }
}
