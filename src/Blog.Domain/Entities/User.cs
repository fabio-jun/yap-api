namespace Blog.Domain.Entities;

// Entity class — represents the "Users" table in the database.
// Each property maps to a column. EF Core uses this class to generate SQL queries.
// This is a POCO (Plain Old CLR Object): no framework dependencies, just data.
public class User
{
    // Primary Key — auto-incremented by the database (int identity column)
    public int Id { get; set; }

    // 'required' keyword (C# 11): forces the property to be set during object initialization.
    // Without it, you could create a User with UserName = null, which would break at the DB level.
    public required string UserName { get; set; }
    public required string  Email { get; set; }

    // Stores the BCrypt hash, never the plain password
    public required string PasswordHash { get; set; }

    // "User" or "Admin" — used in JWT claims for authorization checks
    public required string Role { get; set; }

    // DateTime — maps to PostgreSQL 'timestamp'. Set automatically via HasDefaultValueSql in the configuration.
    public DateTime CreatedAt { get; set; }

    // 'string?' — nullable reference type. The '?' means this column allows NULL in the DB.
    public string? ProfileImageUrl { get; set; }
    public string? DisplayName { get; set; }
    public string? Bio { get; set; }

    // Navigation properties — these don't create columns in the DB.
    // EF Core uses them to represent relationships (JOIN queries).
    // ICollection<T> allows EF Core to lazy-load or eager-load (.Include()) related entities.
    public ICollection<Post>? Posts { get; set; }
    public ICollection<Comment>? Comments { get; set; }
    public ICollection<Like>? Likes { get; set; }

    // A user has many followers (people who follow them)
    public ICollection<Follow>? Followers { get; set; }
    // A user follows many people
    public ICollection<Follow>? Following { get; set; }

    public ICollection<Bookmark>? Bookmarks { get; set; }

    // Direct messages sent and received — two separate collections for the two FK directions
    public ICollection<DirectMessage>? SentMessages { get; set; }
    public ICollection<DirectMessage>? ReceivedMessages { get; set; }
}
