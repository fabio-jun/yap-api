using Blog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Blog.Infrastructure.Configurations;

// EF Core Fluent API configuration for the PostTag join entity.
// This is the explicit join table for the many-to-many relationship between Post and Tag.
public class PostTagConfiguration : IEntityTypeConfiguration<PostTag>
{
    public void Configure(EntityTypeBuilder<PostTag> builder)
    {
        builder.ToTable("PostTags");

        // Composite PK — (PostId, TagId) ensures a tag is linked to a post only once
        builder.HasKey(pt => new { pt.PostId, pt.TagId });

        // PostTag → Post (Many-to-One)
        // Cascade: deleting a post removes all its tag associations
        builder.HasOne(pt => pt.Post)
            .WithMany(p => p.PostTags)
            .HasForeignKey(pt => pt.PostId)
            .OnDelete(DeleteBehavior.Cascade);

        // PostTag → Tag (Many-to-One)
        // Cascade: deleting a tag removes all its post associations
        builder.HasOne(pt => pt.Tag)
            .WithMany(t => t.PostTags)
            .HasForeignKey(pt => pt.TagId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
