using Microsoft.EntityFrameworkCore;
using Vitalis.Application.Interfaces;
using Vitalis.Domain.Entities.Auth;

namespace Vitalis.Infrastructure.Persistence;

// Idempotent: checks before inserting, safe to run on every startup.
public static class DbSeeder
{
    // db.sql's own comment says 4 "original" system roles (before Pharmacist got
    // its own screens in the 24-screen review) — Pharmacist is included here since
    // VC-16/17/18 and VC-22 both need it as a distinct role.
    private static readonly string[] SystemRoles = ["Admin", "Doctor", "Receptionist", "Patient", "Pharmacist"];

    public static async Task SeedAsync(VitalisDbContext context, IPasswordHasher passwordHasher, CancellationToken cancellationToken = default)
    {
        foreach (var roleName in SystemRoles)
        {
            if (!await context.Roles.AnyAsync(r => r.Name == roleName, cancellationToken))
                context.Roles.Add(new Role { Name = roleName, IsSystem = true });
        }
        await context.SaveChangesAsync(cancellationToken);

        if (await context.Users.AnyAsync(u => u.Username == "admin", cancellationToken))
            return;

        // Demo/bootstrap credentials only — change immediately on any real deployment.
        var admin = new User
        {
            Username = "admin",
            PasswordHash = passwordHasher.Hash("Admin@123"),
            FullName = "Quản trị viên hệ thống",
            IsActive = true,
            EmailConfirmed = true,
        };
        context.Users.Add(admin);
        await context.SaveChangesAsync(cancellationToken);

        var adminRole = await context.Roles.FirstAsync(r => r.Name == "Admin", cancellationToken);
        context.UserRoles.Add(new UserRole { UserId = admin.Id, RoleId = adminRole.Id });
        await context.SaveChangesAsync(cancellationToken);
    }
}
