namespace Blog.Application.DTOs.Likes;

public class LikedUserResponse
{
    public int Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    public string? ProfileImageUrl { get; set; }
}
