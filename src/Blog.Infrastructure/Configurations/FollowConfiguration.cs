using Blog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Blog.Infrastructure.Configurations;

// EF Core Fluent API configuration for the Follow entity.
// Models the self-referencing many-to-many relationship: User follows User.
public class FollowConfiguration : IEntityTypeConfiguration<Follow>
{
    public void Configure(EntityTypeBuilder<Follow> builder)
    {
        // Composite PK — (FollowerId, FollowingId) ensures unique follow relationships
        builder.HasKey(f => new { f.FollowerId, f.FollowingId });

        // Follow → Follower (the user who follows)
        // Note: Follower.Following collection — "the user's follow actions"
        builder.HasOne(f => f.Follower)
            .WithMany(u => u.Following)
            .HasForeignKey(f => f.FollowerId)
            .OnDelete(DeleteBehavior.Cascade);
        // SQL equivalent: FOREIGN KEY (FollowerId) REFERENCES Users(Id) ON DELETE CASCADE

        // Follow → Following (the user being followed)
        // Note: Following.Followers collection — "people who follow this user"
        builder.HasOne(f => f.Following)
            .WithMany(u => u.Followers)
            .HasForeignKey(f => f.FollowingId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
