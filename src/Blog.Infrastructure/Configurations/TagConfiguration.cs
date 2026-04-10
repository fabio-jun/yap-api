using Blog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Blog.Infrastructure.Configurations;

// EF Core Fluent API configuration for the Tag entity.
public class TagConfiguration: IEntityTypeConfiguration<Tag>
{
    public void Configure(EntityTypeBuilder<Tag> builder)
    {
        builder.ToTable("Tags");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Name)
            .IsRequired()
            .HasMaxLength(50);

        // Unique index on Name — prevents duplicate tags (e.g., two "dev" tags)
        builder.HasIndex(t => t.Name)
            .IsUnique()
            .HasDatabaseName("IX_Tags_Name_Unique");
    }
}
