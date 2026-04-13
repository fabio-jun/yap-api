using Blog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Blog.Infrastructure.Configurations;

// Implements IEntityTypeConfiguration<User> — auto-discovered by ApplyConfigurationsFromAssembly().
// Define the database schema details 
public class UserConfiguration : IEntityTypeConfiguration<User>
{
    // Configure is called once during model building — defines how User maps to the DB.
    public void Configure(EntityTypeBuilder<User> builder)
    {
        // Maps this entity to the "Users" table
        builder.ToTable("Users");

        // PK
        builder.HasKey(u => u.Id);

        // Column constraints via Fluent API
        // 'u => u.UserName' is a lambda expression pointing to the property
        // given User u, select u.UserName
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
            .HasDefaultValue("User");

        builder.Property(u => u.Bio)
            .HasMaxLength(160);

        builder.Property(u => u.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP"); // SQL expression — evaluated by PostgreSQL at INSERT time

        // Unique indexes — enforce uniqueness at DB level 
        // HasDatabaseName gives a readable name to the index in the DB
        // IX naming convention for index
        builder.HasIndex(u => u.Email)
            .IsUnique()
            .HasDatabaseName("IX_Users_Email");

        builder.HasIndex(u => u.UserName)
            .IsUnique()
            .HasDatabaseName("IX_Users_UserName");
    }
}
