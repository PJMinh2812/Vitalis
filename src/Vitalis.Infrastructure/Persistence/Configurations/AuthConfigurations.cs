using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vitalis.Domain.Entities.Auth;

namespace Vitalis.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> b)
    {
        b.ToTable("users", "auth", t => t.HasCheckConstraint("CK_users_failed_login_count", "failed_login_count >= 0"));
        b.Property(x => x.Username).HasColumnType("varchar(100)");
        b.Property(x => x.PasswordHash).HasColumnType("varchar(255)");
        b.Property(x => x.Email).HasColumnType("varchar(255)");
        b.Property(x => x.FullName).HasColumnType("nvarchar(255)");
        b.Property(x => x.Phone).HasColumnType("varchar(20)");
        b.Property(x => x.AvatarUrl).HasColumnType("varchar(500)");
        b.HasIndex(x => x.Username).IsUnique();
        b.HasIndex(x => x.Email).IsUnique().HasFilter("[email] IS NOT NULL").HasDatabaseName("UX_users_email");
    }
}

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> b)
    {
        b.ToTable("roles", "auth");
        b.Property(x => x.Name).HasColumnType("varchar(50)");
        b.Property(x => x.Description).HasColumnType("nvarchar(255)");
        b.HasIndex(x => x.Name).IsUnique();
    }
}

public class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> b)
    {
        b.ToTable("permissions", "auth");
        b.Property(x => x.Name).HasColumnType("varchar(100)");
        b.HasIndex(x => x.Name).IsUnique();
    }
}

public class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermission>
{
    public void Configure(EntityTypeBuilder<RolePermission> b)
    {
        b.ToTable("role_permissions", "auth");
        b.HasIndex(x => new { x.RoleId, x.PermissionId }).IsUnique().HasDatabaseName("UQ_role_permissions");
        b.HasOne(x => x.Role).WithMany(r => r.RolePermissions).HasForeignKey(x => x.RoleId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.Permission).WithMany(p => p.RolePermissions).HasForeignKey(x => x.PermissionId);
    }
}

public class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
{
    public void Configure(EntityTypeBuilder<UserRole> b)
    {
        b.ToTable("user_roles", "auth");
        b.HasIndex(x => new { x.UserId, x.RoleId }).IsUnique().HasDatabaseName("UQ_user_roles");
        b.HasIndex(x => x.RoleId);
        b.HasOne(x => x.User).WithMany(u => u.UserRoles).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.Role).WithMany(r => r.UserRoles).HasForeignKey(x => x.RoleId);
        b.HasOne(x => x.AssignedByUser).WithMany().HasForeignKey(x => x.AssignedBy);
    }
}

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> b)
    {
        b.ToTable("refresh_tokens", "auth");
        b.Property(x => x.TokenHash).HasColumnType("char(64)");
        b.Property(x => x.ReplacedByTokenHash).HasColumnType("char(64)");
        b.Property(x => x.DeviceInfo).HasColumnType("nvarchar(255)");
        b.Property(x => x.IpAddress).HasColumnType("varchar(45)");
        b.HasIndex(x => x.TokenHash).IsUnique().HasDatabaseName("UX_refresh_tokens_token_hash");
        b.HasIndex(x => x.UserId);
        b.HasOne(x => x.User).WithMany(u => u.RefreshTokens).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> b)
    {
        b.ToTable("audit_logs", "auth");
        b.Property(x => x.Action).HasColumnType("varchar(50)");
        b.Property(x => x.EntityName).HasColumnType("varchar(100)");
        b.HasIndex(x => new { x.EntityName, x.EntityId }).HasDatabaseName("IX_audit_logs_entity");
        b.HasIndex(x => x.UserId);
        b.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId);
    }
}
