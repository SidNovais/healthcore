using HC.LIS.Modules.UserAccess.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HC.LIS.Modules.UserAccess.Infrastructure.Users;

internal class UserEntityTypeConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users", "user_access");

        builder.Property<UserId>("_id")
            .HasColumnName("id")
            .HasConversion(id => id.Value, g => new UserId(g))
            .ValueGeneratedNever();

        builder.HasKey("_id");

        builder.Property<UserEmail>("_email")
            .HasColumnName("email")
            .HasConversion(email => email.Value, s => UserEmail.Of(s))
            .IsRequired()
            .HasMaxLength(255);

        builder.HasIndex("_email").IsUnique();

        builder.Property<string>("_fullName")
            .HasColumnName("full_name")
            .IsRequired()
            .HasMaxLength(255);

        builder.Property<DateTime>("_birthdate")
            .HasColumnName("birthdate")
            .HasColumnType("date");

        builder.Property<string>("_gender")
            .HasColumnName("gender")
            .IsRequired()
            .HasMaxLength(20);

        builder.Property<UserRole>("_role")
            .HasColumnName("role")
            .HasConversion(role => role.Value, s => UserRole.Of(s))
            .IsRequired()
            .HasMaxLength(50);

        builder.Property<UserStatus>("_status")
            .HasColumnName("status")
            .HasConversion(status => status.Value, s => UserStatus.Of(s))
            .IsRequired()
            .HasMaxLength(50);

        builder.Property<string?>("_passwordHash")
            .HasColumnName("password_hash")
            .HasMaxLength(500);

        builder.Property<string?>("_invitationToken")
            .HasColumnName("invitation_token")
            .HasMaxLength(100);

        builder.Property<DateTime?>("_activatedAt")
            .HasColumnName("activated_at");

        builder.Property<DateTime>("_createdAt")
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property<Guid?>("_createdById")
            .HasColumnName("created_by_id");
    }
}
