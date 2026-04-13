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
        // Keep the existing database column name while exposing clearer C# property names.
        builder.Property(f => f.FollowedId)
            .HasColumnName("FollowingId");

        // Composite PK: one follower can follow a given user only once.
        builder.HasKey(f => new { f.FollowerId, f.FollowedId });

        builder.HasIndex(f => f.FollowedId)
            .HasDatabaseName("IX_Follows_FollowingId");

        // Follow -> Follower: the user who follows.
        builder.HasOne(f => f.Follower)
            .WithMany(u => u.Following)
            .HasForeignKey(f => f.FollowerId)
            .HasConstraintName("FK_Follows_Users_FollowerId")
            .OnDelete(DeleteBehavior.Cascade);

        // Follow -> Followed: the user being followed.
        builder.HasOne(f => f.Followed)
            .WithMany(u => u.Followers)
            .HasForeignKey(f => f.FollowedId)
            .HasConstraintName("FK_Follows_Users_FollowingId")
            .OnDelete(DeleteBehavior.Cascade);
    }
}
