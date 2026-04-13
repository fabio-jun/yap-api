namespace Blog.Application.DTOs.Blocks;

public class BlockedUserResponse
{
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string? ProfileImageUrl { get; set; }
    public DateTime CreatedAt { get; set; }
}
