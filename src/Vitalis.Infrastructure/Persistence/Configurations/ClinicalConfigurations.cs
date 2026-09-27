using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vitalis.Domain.Entities.Clinical;

namespace Vitalis.Infrastructure.Persistence.Configurations;

public class MedicalRecordConfiguration : IEntityTypeConfiguration<MedicalRecord>
{
    public void Configure(EntityTypeBuilder<MedicalRecord> b)
    {
        b.ToTable("medical_records", "clinical", t => t.HasCheckConstraint("CK_medical_records_status", "status BETWEEN 0 AND 1"));
        b.Property(x => x.Icd10Code).HasColumnType("varchar(10)");
        b.HasOne(x => x.Appointment).WithOne().HasForeignKey<MedicalRecord>(x => x.AppointmentId);
        b.HasOne(x => x.Patient).WithMany().HasForeignKey(x => x.PatientId);
        b.HasOne(x => x.Doctor).WithMany().HasForeignKey(x => x.DoctorId);
        b.HasIndex(x => x.PatientId);
        b.HasIndex(x => x.DoctorId);
    }
}

public class MedicalRecordServiceConfiguration : IEntityTypeConfiguration<MedicalRecordService>
{
    public void Configure(EntityTypeBuilder<MedicalRecordService> b)
    {
        b.ToTable("medical_record_services", "clinical", t =>
        {
            t.HasCheckConstraint("CK_mrs_quantity", "quantity > 0");
            t.HasCheckConstraint("CK_mrs_unit_price", "unit_price_snapshot >= 0");
            t.HasCheckConstraint("CK_mrs_status", "status BETWEEN 0 AND 3");
        });
        b.HasOne(x => x.MedicalRecord).WithMany(m => m.Services).HasForeignKey(x => x.MedicalRecordId);
        b.HasOne(x => x.Service).WithMany().HasForeignKey(x => x.ServiceId);
        b.HasOne(x => x.PerformedByUser).WithMany().HasForeignKey(x => x.PerformedBy);
        b.HasIndex(x => new { x.MedicalRecordId, x.Status }).HasDatabaseName("IX_mrs_medical_record_status");
    }
}

public class LabResultConfiguration : IEntityTypeConfiguration<LabResult>
{
    public void Configure(EntityTypeBuilder<LabResult> b)
    {
        b.ToTable("lab_results", "clinical");
        b.Property(x => x.ResultValue).HasColumnType("nvarchar(500)");
        b.Property(x => x.ReferenceRange).HasColumnType("nvarchar(255)");
        b.Property(x => x.Conclusion).HasColumnType("nvarchar(500)");
        b.HasOne(x => x.MedicalRecordService).WithMany(s => s.LabResults).HasForeignKey(x => x.MedicalRecordServiceId);
        b.HasIndex(x => x.MedicalRecordServiceId).HasDatabaseName("IX_lab_results_mrs_id");
    }
}

public class AttachmentConfiguration : IEntityTypeConfiguration<Attachment>
{
    public void Configure(EntityTypeBuilder<Attachment> b)
    {
        b.ToTable("attachments", "clinical");
        b.Property(x => x.FileUrl).HasColumnType("nvarchar(500)");
        b.Property(x => x.FileType).HasColumnType("varchar(50)");
        b.HasOne(x => x.MedicalRecord).WithMany(m => m.Attachments).HasForeignKey(x => x.MedicalRecordId);
        b.HasIndex(x => x.MedicalRecordId);
    }
}

public class PatientVitalsConfiguration : IEntityTypeConfiguration<PatientVitals>
{
    public void Configure(EntityTypeBuilder<PatientVitals> b)
    {
        b.ToTable("patient_vitals", "clinical");
        b.Property(x => x.Temperature).HasPrecision(4, 1);
        b.Property(x => x.Weight).HasPrecision(5, 2);
        b.Property(x => x.Height).HasPrecision(5, 2);
        b.Property(x => x.BloodPressure).HasColumnType("varchar(20)");
        b.HasOne(x => x.MedicalRecord).WithOne(m => m.Vitals).HasForeignKey<PatientVitals>(x => x.MedicalRecordId);
    }
}

public class PatientAllergyConfiguration : IEntityTypeConfiguration<PatientAllergy>
{
    public void Configure(EntityTypeBuilder<PatientAllergy> b)
    {
        b.ToTable("patient_allergies", "clinical", t => t.HasCheckConstraint("CK_patient_allergies_severity", "severity BETWEEN 0 AND 2"));
        b.Property(x => x.Allergen).HasColumnType("nvarchar(255)");
        b.Property(x => x.Note).HasColumnType("nvarchar(255)");
        b.HasOne(x => x.Patient).WithMany(p => p.Allergies).HasForeignKey(x => x.PatientId);
        b.HasIndex(x => x.PatientId);
    }
}

public class MedicineConfiguration : IEntityTypeConfiguration<Medicine>
{
    public void Configure(EntityTypeBuilder<Medicine> b)
    {
        b.ToTable("medicines", "clinical", t =>
        {
            t.HasCheckConstraint("CK_medicines_stock_non_negative", "stock_quantity >= 0");
            t.HasCheckConstraint("CK_medicines_price", "price >= 0");
        });
        b.Property(x => x.Code).HasColumnType("varchar(30)");
        b.Property(x => x.Name).HasColumnType("nvarchar(255)");
        b.Property(x => x.ActiveIngredient).HasColumnType("nvarchar(255)");
        b.Property(x => x.Concentration).HasColumnType("nvarchar(100)");
        b.Property(x => x.Unit).HasColumnType("nvarchar(50)");
        b.Property(x => x.Description).HasColumnType("nvarchar(500)");
        b.HasIndex(x => x.Code).IsUnique().HasFilter("[code] IS NOT NULL").HasDatabaseName("UX_medicines_code");
    }
}

public class MedicineBatchConfiguration : IEntityTypeConfiguration<MedicineBatch>
{
    public void Configure(EntityTypeBuilder<MedicineBatch> b)
    {
        b.ToTable("medicine_batches", "clinical", t => t.HasCheckConstraint("CK_medicine_batches_quantity", "quantity >= 0"));
        b.Property(x => x.BatchNo).HasColumnType("varchar(50)");
        b.HasOne(x => x.Medicine).WithMany(m => m.Batches).HasForeignKey(x => x.MedicineId);
        b.HasIndex(x => new { x.MedicineId, x.BatchNo }).IsUnique().HasDatabaseName("UX_medicine_batches_medicine_batch_no");
        b.HasIndex(x => x.ExpiryDate).HasDatabaseName("IX_medicine_batches_expiry_date");
    }
}

public class PrescriptionConfiguration : IEntityTypeConfiguration<Prescription>
{
    public void Configure(EntityTypeBuilder<Prescription> b)
    {
        b.ToTable("prescriptions", "clinical", t => t.HasCheckConstraint("CK_prescriptions_status", "status BETWEEN 0 AND 1"));
        b.Property(x => x.Note).HasColumnType("nvarchar(500)");
        b.HasOne(x => x.MedicalRecord).WithOne(m => m.Prescription).HasForeignKey<Prescription>(x => x.MedicalRecordId);
        b.HasOne(x => x.Doctor).WithMany().HasForeignKey(x => x.DoctorId);
        b.HasOne(x => x.DispensedByUser).WithMany().HasForeignKey(x => x.DispensedBy);
    }
}

public class PrescriptionItemConfiguration : IEntityTypeConfiguration<PrescriptionItem>
{
    public void Configure(EntityTypeBuilder<PrescriptionItem> b)
    {
        b.ToTable("prescription_items", "clinical", t => t.HasCheckConstraint("CK_prescription_items_quantity", "quantity > 0"));
        b.Property(x => x.MedicineNameSnapshot).HasColumnType("nvarchar(255)");
        b.Property(x => x.Dosage).HasColumnType("nvarchar(255)");
        b.Property(x => x.Frequency).HasColumnType("nvarchar(50)");
        b.Property(x => x.Instruction).HasColumnType("nvarchar(255)");
        b.HasOne(x => x.Prescription).WithMany(p => p.Items).HasForeignKey(x => x.PrescriptionId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.Medicine).WithMany().HasForeignKey(x => x.MedicineId);
        b.HasIndex(x => x.PrescriptionId);
        b.HasIndex(x => x.MedicineId);
    }
}

public class MedicineStockTransactionConfiguration : IEntityTypeConfiguration<MedicineStockTransaction>
{
    public void Configure(EntityTypeBuilder<MedicineStockTransaction> b)
    {
        b.ToTable("medicine_stock_transactions", "clinical", t =>
        {
            t.HasCheckConstraint("CK_mst_type", "type BETWEEN 0 AND 2");
            t.HasCheckConstraint("CK_mst_quantity", "quantity > 0");
        });
        b.HasOne(x => x.Medicine).WithMany().HasForeignKey(x => x.MedicineId);
        b.HasOne(x => x.Batch).WithMany().HasForeignKey(x => x.BatchId);
        b.HasOne(x => x.Prescription).WithMany().HasForeignKey(x => x.PrescriptionId);
        b.HasOne(x => x.CreatedByUser).WithMany().HasForeignKey(x => x.CreatedBy);
        b.HasIndex(x => x.MedicineId);
        b.HasIndex(x => x.BatchId);
    }
}
