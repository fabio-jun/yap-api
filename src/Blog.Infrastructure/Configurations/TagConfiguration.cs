using Blog.Domain.Entities;                                                                                                     
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Blog.Infrastructure.Configurations;

public class TagConfiguration: IEntityTypeConfiguration<Tag>
{
    public void Configure(EntityTypeBuilder<Tag> builder)
    {
        // Table name
        builder.ToTable("Tags");

        // PK
        builder.HasKey(t => t.Id);

        // Properties
        builder.Property(t => t.Name)
            .IsRequired()
            .HasMaxLength(50);

        // Unique index on Name
        builder.Property(t => t.Name)
            .IsRequired()
            .HasMaxLength(50);

        // Unique index on Name
        builder.HasIndex(t => t.Name)
            .IsUnique()
            .HasDatabaseName("IX_Tags_Name_Unique");
    }
}