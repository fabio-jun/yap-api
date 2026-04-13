using System.ComponentModel.Design.Serialization;

namespace Blog.Domain.Entities;

// Each property maps to a column. EF Core uses this class to generate SQL queries.
// Plain Old CLR Object(POCO): no framework dependencies, just data.
public class User
{
    public int Id { get; set; }
    // required keyword - forces the property to be set during object initialization.
    public required string UserName { get; set; }
    public required string  Email { get; set; }

    // Stores the BCrypt hash, never the plain password
    public required string PasswordHash { get; set; }

    // User or Admin — used in JWT claims for authorization checks
    public required string Role { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? ProfileImageUrl { get; set; }
    public string? DisplayName { get; set; }
    public string? Bio { get; set; }

    // Navigation properties — don't create columns in the DB.
    // EF Core uses them to represent relationships (JOIN queries).
    // ICollection<T> interface for a collection of related entities. In this context is used to represent the "many" side of a relationship
    public ICollection<Post>? Posts { get; set; } // relationship 1:N
    public ICollection<Comment>? Comments { get; set; }
    public ICollection<Like>? Likes { get; set; }
    public ICollection<Follow>? Followers { get; set; }
    public ICollection<Follow>? Following { get; set; }
    public ICollection<Bookmark>? Bookmarks { get; set; }
    public ICollection<DirectMessage>? SentMessages { get; set; }
    public ICollection<DirectMessage>? ReceivedMessages { get; set; }
}
