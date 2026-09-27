using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Vitalis.Domain.Entities.Auth;
using Vitalis.Domain.Entities.Billing;
using Vitalis.Domain.Entities.Clinical;
using Vitalis.Domain.Entities.Scheduling;

namespace Vitalis.Infrastructure.Persistence;

public class VitalisDbContext(DbContextOptions<VitalisDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    public DbSet<Specialty> Specialties => Set<Specialty>();
    public DbSet<Doctor> Doctors => Set<Doctor>();
    public DbSet<Patient> Patients => Set<Patient>();
    public DbSet<DoctorSchedule> DoctorSchedules => Set<DoctorSchedule>();
    public DbSet<DoctorTimeOff> DoctorTimeOffs => Set<DoctorTimeOff>();
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<AppointmentStatusHistory> AppointmentStatusHistories => Set<AppointmentStatusHistory>();
    public DbSet<Notification> Notifications => Set<Notification>();

    public DbSet<MedicalRecord> MedicalRecords => Set<MedicalRecord>();
    public DbSet<MedicalRecordService> MedicalRecordServices => Set<MedicalRecordService>();
    public DbSet<LabResult> LabResults => Set<LabResult>();
    public DbSet<Attachment> Attachments => Set<Attachment>();
    public DbSet<PatientVitals> PatientVitals => Set<PatientVitals>();
    public DbSet<PatientAllergy> PatientAllergies => Set<PatientAllergy>();
    public DbSet<Medicine> Medicines => Set<Medicine>();
    public DbSet<MedicineBatch> MedicineBatches => Set<MedicineBatch>();
    public DbSet<Prescription> Prescriptions => Set<Prescription>();
    public DbSet<PrescriptionItem> PrescriptionItems => Set<PrescriptionItem>();
    public DbSet<MedicineStockTransaction> MedicineStockTransactions => Set<MedicineStockTransaction>();

    public DbSet<Service> Services => Set<Service>();
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<InvoiceItem> InvoiceItems => Set<InvoiceItem>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<InsurancePolicy> InsurancePolicies => Set<InsurancePolicy>();

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<decimal>().HavePrecision(12, 2);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.UseCollation("Vietnamese_CI_AS");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(VitalisDbContext).Assembly);

        foreach (var entity in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entity.GetProperties())
            {
                property.SetColumnName(ToSnakeCase(property.Name));

                if (property.Name is "CreatedAt" or "AssignedAt" or "OrderedAt" or "UploadedAt" or "ChangedAt" or "PaidAt")
                    property.SetDefaultValueSql("SYSUTCDATETIME()");
            }

            // EF's default is Cascade, which yields multiple cascade paths into medical_records (Msg 1785).
            // Restrict everywhere except the relationships explicitly set to Cascade in the configurations.
            foreach (var fk in entity.GetForeignKeys())
            {
                if (((IConventionForeignKey)fk).GetDeleteBehaviorConfigurationSource() != ConfigurationSource.Explicit)
                    fk.DeleteBehavior = DeleteBehavior.Restrict;
            }
        }
    }

    private static string ToSnakeCase(string name) =>
        Regex.Replace(name, "([a-z0-9])([A-Z])", "$1_$2").ToLowerInvariant();
}
