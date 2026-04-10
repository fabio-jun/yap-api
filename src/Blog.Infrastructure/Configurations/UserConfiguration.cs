using Blog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Blog.Infrastructure.Configurations;

// EF Core Fluent API configuration for the User entity.
// Implements IEntityTypeConfiguration<User> — auto-discovered by ApplyConfigurationsFromAssembly().
// This is where you define the database schema details that can't be expressed by the entity alone:
// table name, column constraints, indexes, relationships, default values.
public class UserConfiguration : IEntityTypeConfiguration<User>
{
    // Configure is called once during model building — defines how User maps to the DB.
    public void Configure(EntityTypeBuilder<User> builder)
    {
        // Maps this entity to the "Users" table
        builder.ToTable("Users");

        // Primary Key — EF Core would auto-detect "Id" by convention, but explicit is clearer
        builder.HasKey(u => u.Id);

        // Column constraints via Fluent API
        // 'u => u.UserName' is a lambda expression pointing to the property
        builder.Property(u => u.UserName)
            .IsRequired()       // NOT NULL constraint in the database
            .HasMaxLength(50);  // VARCHAR(50) — limits the column size

        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(u => u.PasswordHash)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(u => u.Role)
            .IsRequired()
            .HasMaxLength(20)
            .HasDefaultValue("User"); // DEFAULT 'User' — if not specified during INSERT

        builder.Property(u => u.Bio)
            .HasMaxLength(160);

        builder.Property(u => u.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP"); // SQL expression — evaluated by PostgreSQL at INSERT time

        // Unique indexes — enforce uniqueness at the database level (not just C#)
        // HasDatabaseName gives a readable name to the index in the DB
        builder.HasIndex(u => u.Email)
            .IsUnique()
            .HasDatabaseName("IX_Users_Email");

        builder.HasIndex(u => u.UserName)
            .IsUnique()
            .HasDatabaseName("IX_Users_UserName");
    }
}
