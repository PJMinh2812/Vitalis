using Microsoft.EntityFrameworkCore;
using Vitalis.Application.Interfaces;
using Vitalis.Domain.Entities.Auth;
using Vitalis.Domain.Entities.Scheduling;

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

        // One demo account per role — bootstrap credentials only, change on any
        // real deployment. Also the ONLY way a Receptionist/Pharmacist account
        // ever gets created: unlike Patient (self-register) and Doctor
        // (admin/doctors), there's no admin endpoint to create those two roles.
        await SeedUserIfMissingAsync(context, passwordHasher, "admin", "Quản trị viên hệ thống", "Admin", "Admin@123", cancellationToken);
        await SeedUserIfMissingAsync(context, passwordHasher, "reception", "Lễ tân Hoàng Mai", "Receptionist", "Reception@123", cancellationToken);
        await SeedUserIfMissingAsync(context, passwordHasher, "pharmacist", "Dược sĩ Ngân Anh", "Pharmacist", "Pharmacist@123", cancellationToken);

        var doctorUser = await SeedUserIfMissingAsync(context, passwordHasher, "doctor", "BS. Minh Trí", "Doctor", "Doctor@123", cancellationToken);
        if (doctorUser is not null)
        {
            var specialty = await context.Specialties.FirstOrDefaultAsync(s => s.Code == "DK", cancellationToken);
            if (specialty is null)
            {
                specialty = new Specialty { Name = "Đa khoa", Code = "DK", IsActive = true };
                context.Specialties.Add(specialty);
                await context.SaveChangesAsync(cancellationToken);
            }

            context.Doctors.Add(new Doctor
            {
                UserId = doctorUser.Id,
                SpecialtyId = specialty.Id,
                FullName = doctorUser.FullName!,
                ConsultationFee = 150_000,
                IsActive = true,
            });
            await context.SaveChangesAsync(cancellationToken);
        }

        var patientUser = await SeedUserIfMissingAsync(context, passwordHasher, "patient", "Nguyễn Văn An", "Patient", "Patient@123", cancellationToken);
        if (patientUser is not null)
        {
            var patient = new Patient
            {
                UserId = patientUser.Id,
                FullName = patientUser.FullName!,
                Phone = "0900000000",
                IsActive = true,
            };
            context.Patients.Add(patient);
            await context.SaveChangesAsync(cancellationToken);

            // Same two-phase pattern as PatientService — derives the code from the
            // IDENTITY value SQL Server just assigned. No concurrency risk here
            // (seeding runs once), but keeping the pattern consistent either way.
            patient.PatientCode = $"BN{patient.Id:D4}";
            await context.SaveChangesAsync(cancellationToken);
        }
    }

    // Returns the created User, or null if that username already existed (nothing to do).
    private static async Task<User?> SeedUserIfMissingAsync(
        VitalisDbContext context, IPasswordHasher passwordHasher, string username, string fullName, string roleName, string password, CancellationToken cancellationToken)
    {
        if (await context.Users.AnyAsync(u => u.Username == username, cancellationToken))
            return null;

        var user = new User
        {
            Username = username,
            PasswordHash = passwordHasher.Hash(password),
            FullName = fullName,
            IsActive = true,
            EmailConfirmed = true,
        };
        context.Users.Add(user);
        await context.SaveChangesAsync(cancellationToken);

        var role = await context.Roles.FirstAsync(r => r.Name == roleName, cancellationToken);
        context.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = role.Id });
        await context.SaveChangesAsync(cancellationToken);

        return user;
    }
}
