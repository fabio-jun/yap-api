namespace Blog.Domain.Entities;

public class User
{
    public int Id { get; set; }
    public required string UserName { get; set; }
    public required string  Email { get; set; }
    public required string PasswordHash { get; set; }
    public required string Role { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? ProfileImageUrl { get; set; }
    public string? Bio { get; set; }
    public ICollection<Post>? Posts { get; set; }
    public ICollection<Comment>? Comments { get; set; }
    public ICollection<Like>? Likes { get; set; }
    public ICollection<Follow>? Followers { get; set; }
    public ICollection<Follow>? Following { get; set; }
    public ICollection<Bookmark>? Bookmarks { get; set; }

}