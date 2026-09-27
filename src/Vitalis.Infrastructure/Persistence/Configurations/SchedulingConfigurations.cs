using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vitalis.Domain.Entities.Scheduling;

namespace Vitalis.Infrastructure.Persistence.Configurations;

public class SpecialtyConfiguration : IEntityTypeConfiguration<Specialty>
{
    public void Configure(EntityTypeBuilder<Specialty> b)
    {
        b.ToTable("specialties", "scheduling");
        b.Property(x => x.Code).HasColumnType("varchar(20)");
        b.Property(x => x.Name).HasColumnType("nvarchar(255)");
        b.Property(x => x.Description).HasColumnType("nvarchar(500)");
        b.HasIndex(x => x.Code).IsUnique().HasFilter("[code] IS NOT NULL").HasDatabaseName("UX_specialties_code");
    }
}

public class DoctorConfiguration : IEntityTypeConfiguration<Doctor>
{
    public void Configure(EntityTypeBuilder<Doctor> b)
    {
        b.ToTable("doctors", "scheduling", t => t.HasCheckConstraint("CK_doctors_consultation_fee", "consultation_fee IS NULL OR consultation_fee >= 0"));
        b.Property(x => x.FullName).HasColumnType("nvarchar(255)");
        b.Property(x => x.Title).HasColumnType("nvarchar(50)");
        b.Property(x => x.LicenseNumber).HasColumnType("varchar(50)");
        b.Property(x => x.Phone).HasColumnType("varchar(20)");
        b.Property(x => x.Email).HasColumnType("varchar(255)");
        b.Property(x => x.Room).HasColumnType("varchar(50)");
        b.Property(x => x.AvatarUrl).HasColumnType("varchar(500)");
        b.HasOne(x => x.User).WithOne().HasForeignKey<Doctor>(x => x.UserId);
        b.HasOne(x => x.Specialty).WithMany(s => s.Doctors).HasForeignKey(x => x.SpecialtyId);
        b.HasIndex(x => x.SpecialtyId);
        b.HasIndex(x => x.LicenseNumber).IsUnique().HasFilter("[license_number] IS NOT NULL").HasDatabaseName("UX_doctors_license_number");
    }
}

public class PatientConfiguration : IEntityTypeConfiguration<Patient>
{
    public void Configure(EntityTypeBuilder<Patient> b)
    {
        b.ToTable("patients", "scheduling", t => t.HasCheckConstraint("CK_patients_gender", "gender BETWEEN 0 AND 2"));
        b.Property(x => x.PatientCode).HasColumnType("varchar(20)");
        b.Property(x => x.FullName).HasColumnType("nvarchar(255)");
        b.Property(x => x.Phone).HasColumnType("varchar(20)");
        b.Property(x => x.Email).HasColumnType("varchar(255)");
        b.Property(x => x.Address).HasColumnType("nvarchar(500)");
        b.Property(x => x.NationalId).HasColumnType("varchar(20)");
        b.Property(x => x.InsuranceNumber).HasColumnType("varchar(20)");
        b.Property(x => x.BloodType).HasColumnType("varchar(5)");
        b.Property(x => x.EmergencyContactName).HasColumnType("nvarchar(255)");
        b.Property(x => x.EmergencyContactPhone).HasColumnType("varchar(20)");
        b.HasOne(x => x.User).WithOne().HasForeignKey<Patient>(x => x.UserId);
        b.HasIndex(x => x.UserId).IsUnique().HasFilter("[user_id] IS NOT NULL").HasDatabaseName("UX_patients_user_id");
        b.HasIndex(x => x.PatientCode).IsUnique().HasFilter("[patient_code] IS NOT NULL").HasDatabaseName("UX_patients_patient_code");
        b.HasIndex(x => x.FullName);
        b.HasIndex(x => x.Phone);
    }
}

public class DoctorScheduleConfiguration : IEntityTypeConfiguration<DoctorSchedule>
{
    public void Configure(EntityTypeBuilder<DoctorSchedule> b)
    {
        b.ToTable("doctor_schedules", "scheduling", t =>
        {
            t.HasCheckConstraint("CK_doctor_schedules_time", "end_time > start_time");
            t.HasCheckConstraint("CK_doctor_schedules_day_of_week", "day_of_week BETWEEN 0 AND 6");
            t.HasCheckConstraint("CK_doctor_schedules_break", "(break_start IS NULL AND break_end IS NULL) OR (break_end > break_start)");
            t.HasCheckConstraint("CK_doctor_schedules_effective", "effective_to IS NULL OR effective_to > effective_from");
        });
        b.Property(x => x.DayOfWeek).HasConversion<byte>();
        b.HasOne(x => x.Doctor).WithMany(d => d.Schedules).HasForeignKey(x => x.DoctorId);
        b.HasIndex(x => x.DoctorId);
        b.HasIndex(x => new { x.DoctorId, x.DayOfWeek, x.StartTime }).IsUnique().HasDatabaseName("UX_doctor_schedules_slot");
    }
}

public class DoctorTimeOffConfiguration : IEntityTypeConfiguration<DoctorTimeOff>
{
    public void Configure(EntityTypeBuilder<DoctorTimeOff> b)
    {
        b.ToTable("doctor_time_off", "scheduling", t =>
        {
            t.HasCheckConstraint("CK_doctor_time_off_time", "end_at > start_at");
            t.HasCheckConstraint("CK_doctor_time_off_type", "type BETWEEN 0 AND 2");
        });
        b.Property(x => x.Reason).HasColumnType("nvarchar(255)");
        b.HasOne(x => x.Doctor).WithMany().HasForeignKey(x => x.DoctorId);
        b.HasOne(x => x.ApprovedByUser).WithMany().HasForeignKey(x => x.ApprovedBy);
        b.HasIndex(x => new { x.DoctorId, x.StartAt }).HasDatabaseName("IX_doctor_time_off_doctor_start");
    }
}

public class AppointmentConfiguration : IEntityTypeConfiguration<Appointment>
{
    public void Configure(EntityTypeBuilder<Appointment> b)
    {
        b.ToTable("appointments", "scheduling", t =>
        {
            t.HasCheckConstraint("CK_appointments_time", "end_time > start_time");
            t.HasCheckConstraint("CK_appointments_status", "status BETWEEN 0 AND 6");
            t.HasCheckConstraint("CK_appointments_source", "source BETWEEN 0 AND 2");
            t.HasCheckConstraint("CK_appointments_fee_snapshot", "fee_snapshot IS NULL OR fee_snapshot >= 0");
        });
        b.Property(x => x.AppointmentCode).HasColumnType("varchar(20)");
        b.Property(x => x.Reason).HasColumnType("nvarchar(500)");
        b.Property(x => x.CancelReason).HasColumnType("nvarchar(500)");
        b.Property(x => x.Note).HasColumnType("nvarchar(500)");
        b.HasOne(x => x.Patient).WithMany().HasForeignKey(x => x.PatientId);
        b.HasOne(x => x.Doctor).WithMany().HasForeignKey(x => x.DoctorId);
        b.HasOne(x => x.CreatedByUser).WithMany().HasForeignKey(x => x.CreatedBy);
        b.HasIndex(x => new { x.DoctorId, x.StartTime }).HasDatabaseName("IX_appointments_doctor_start");
        b.HasIndex(x => x.PatientId);
        b.HasIndex(x => x.StartTime);
        b.HasIndex(x => x.AppointmentCode).IsUnique().HasFilter("[appointment_code] IS NOT NULL").HasDatabaseName("UX_appointments_code");
        // status < 5 covers every state that still holds the slot; Cancelled (5) and NoShow (6) free it
        b.HasIndex(x => new { x.DoctorId, x.StartTime }).IsUnique().HasFilter("[status] < 5").HasDatabaseName("UX_appointments_doctor_slot");
    }
}

public class AppointmentStatusHistoryConfiguration : IEntityTypeConfiguration<AppointmentStatusHistory>
{
    public void Configure(EntityTypeBuilder<AppointmentStatusHistory> b)
    {
        b.ToTable("appointment_status_history", "scheduling", t =>
        {
            t.HasCheckConstraint("CK_status_history_from_status", "from_status BETWEEN 0 AND 6");
            t.HasCheckConstraint("CK_status_history_to_status", "to_status BETWEEN 0 AND 6");
        });
        b.Property(x => x.Reason).HasColumnType("nvarchar(500)");
        b.HasOne(x => x.Appointment).WithMany(a => a.StatusHistory).HasForeignKey(x => x.AppointmentId);
        b.HasOne(x => x.ChangedByUser).WithMany().HasForeignKey(x => x.ChangedBy);
        b.HasIndex(x => x.AppointmentId).HasDatabaseName("IX_status_history_appointment_id");
    }
}

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> b)
    {
        b.ToTable("notifications", "scheduling", t =>
        {
            t.HasCheckConstraint("CK_notifications_channel", "channel BETWEEN 0 AND 2");
            t.HasCheckConstraint("CK_notifications_status", "status BETWEEN 0 AND 2");
        });
        b.HasOne(x => x.Appointment).WithMany().HasForeignKey(x => x.AppointmentId);
        b.HasIndex(x => x.AppointmentId);
    }
}
