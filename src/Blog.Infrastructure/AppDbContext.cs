using Blog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace Blog.Infrastructure;

public class AppDbContext : DbContext
{
    // Constructor — receives DbContextOptions containing the connection string and provider (PostgreSQL).
    // base(options)- passes the options to the parent DbContext constructor.
    // The options are configured in Program.cs via builder.Services.AddDbContext<AppDbContext>(...).
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    // DbSet<T> properties — each one represents a table in the database.
    // _context.Users becomes "SELECT ... FROM Users" when queried with LINQ.
    // _context.Users.AddAsync(user) becomes "INSERT INTO Users (...)".
    public DbSet<User> Users { get; set; }
    public DbSet<Post> Posts { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<Comment> Comments { get; set; }
    public DbSet<Tag> Tags { get; set; }
    public DbSet<PostTag> PostTags { get; set; }
    public DbSet<Like> Likes { get; set; }
    public DbSet<Follow> Follows { get; set; }
    public DbSet<Bookmark> Bookmarks { get; set; }
    public DbSet<DirectMessage> DirectMessages { get; set; }
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<Repost> Reposts { get; set; }
    public DbSet<BlockedUser> BlockedUsers { get; set; }
    public DbSet<Report> Reports { get; set; }

    // OnModelCreating is called when EF Core builds the internal model of the database.
    // This is where you configure relationships, constraints, indexes, etc. using Fluent API.
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Calls the base implementation (required for proper initialization)
        base.OnModelCreating(modelBuilder);

        // ApplyConfigurationsFromAssembly scans this assembly (Blog.Infrastructure) for all classes
        // that implement IEntityTypeConfiguration<T> and applies them automatically.
        // This means each entity's configuration lives in its own file (e.g., UserConfiguration.cs)
        // instead of cluttering OnModelCreating with hundreds of lines.
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
