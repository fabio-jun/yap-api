using Blog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Blog.Infrastructure.Configurations;

// EF Core Fluent API configuration for the Like entity.
public class LikeConfiguration : IEntityTypeConfiguration<Like>
{
    public void Configure(EntityTypeBuilder<Like> builder)
    {
        builder.ToTable("Likes");

        // Composite Primary Key — two columns together form the PK.
        // This ensures a user can only like a post once (unique constraint at DB level).
        // 'new { l.PostId, l.UserId }' — anonymous object with both FK fields.
        builder.HasKey(l => new { l.PostId, l.UserId });

        builder.Property(l => l.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        // Like → Post (Many-to-One) — a post can have many likes
        // Cascade: deleting a post deletes all its likes
        builder.HasOne(l => l.Post)
            .WithMany(p => p.Likes)
            .HasForeignKey(l => l.PostId)
            .OnDelete(DeleteBehavior.Cascade);

        // Like → User (Many-to-One) — a user can have many likes
        builder.HasOne(l => l.User)
            .WithMany(u => u.Likes)
            .HasForeignKey(l => l.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
