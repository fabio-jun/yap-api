using Blog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Blog.Infrastructure.Configurations;

// Refresh tokens support JWT token rotation — when the access token expires,
// the client exchanges a valid refresh token for a new access + refresh token pair.
public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("RefreshTokens");

        builder.HasKey(r => r.Id);

        // The token string itself 
        builder.Property(r => r.Token)
            .IsRequired();

        // Expiration timestamp
        builder.Property(r => r.ExpiresAt)
            .IsRequired();

        // Revocation flag — set to true when the token is used (rotation) or manually revoked
        builder.Property(r => r.IsRevoked)
            .IsRequired();

        builder.Property(r => r.UserId)
            .IsRequired();

        // RefreshToken → User (Many-to-One)
        // WithMany() with no argument — User entity doesn't have a navigation collection for refresh tokens
        // Cascade: deleting a user deletes all their refresh tokens
        builder.HasOne(r => r.User)
            .WithMany()
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
