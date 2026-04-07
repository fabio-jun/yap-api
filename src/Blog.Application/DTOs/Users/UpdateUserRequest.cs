namespace Blog.Application.DTOs.Users;

public class UpdateUserRequest
{
    public required string userName { get; set; }
    public required string Email { get; set; }
    public string? ProfileImageUrl { get; set; }
    public string? Bio { get; set; }
}

