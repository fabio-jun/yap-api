using Blog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace Blog.Infrastructure;

//DbContext: EF Core's base class
public class AppDbContext : DbContext
{
    // Constructor receives the connection configurations
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    // Set tables as object collections in C#
    public DbSet<User> Users { get; set; }
    public DbSet<Post> Posts { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<Comment> Comments { get; set; }
    public DbSet<Tag> Tags { get; set; }
    public DbSet<PostTag> PostTags { get; set; }
    public DbSet<Like> Likes { get; set; }
    public DbSet<Follow> Follows { get; set; }
    public DbSet<Bookmark> Bookmarks { get; set; }

    //Apply the entities's classes configurations
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Calls the original implementation from the father class
        base.OnModelCreating(modelBuilder);
        //Automatically searches for all the classes that implement IEntityTypeConfiguration<T> in the
        //project and applies
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
