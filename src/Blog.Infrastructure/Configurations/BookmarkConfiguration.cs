using Blog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Blog.Infrastructure.Configurations;

// EF Core Fluent API configuration for the Bookmark entity.
// Same composite PK pattern as Like — (PostId, UserId) ensures unique bookmarks.
public class BookmarkConfiguration : IEntityTypeConfiguration<Bookmark>
{
    public void Configure(EntityTypeBuilder<Bookmark> builder)
    {
        // Composite PK — a user can only bookmark a post once
        builder.HasKey(b => new { b.PostId, b.UserId });

        // Bookmark → Post (Many-to-One)
        builder.HasOne(b => b.Post)
            .WithMany(p => p.Bookmarks)
            .HasForeignKey(b => b.PostId)
            .OnDelete(DeleteBehavior.Cascade);

        // Bookmark → User (Many-to-One)
        builder.HasOne(b => b.User)
            .WithMany(u => u.Bookmarks)
            .HasForeignKey(b => b.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
