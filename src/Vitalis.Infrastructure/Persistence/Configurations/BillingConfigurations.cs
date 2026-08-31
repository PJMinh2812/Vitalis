using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vitalis.Domain.Entities.Billing;

namespace Vitalis.Infrastructure.Persistence.Configurations;

public class ServiceConfiguration : IEntityTypeConfiguration<Service>
{
    public void Configure(EntityTypeBuilder<Service> b)
    {
        b.ToTable("services", "billing", t => t.HasCheckConstraint("CK_services_price", "price >= 0"));
        b.Property(x => x.Code).HasColumnType("varchar(20)");
        b.Property(x => x.Name).HasColumnType("nvarchar(255)");
        b.Property(x => x.Description).HasColumnType("nvarchar(500)");
        b.HasOne(x => x.Specialty).WithMany().HasForeignKey(x => x.SpecialtyId);
        b.HasIndex(x => x.Code).IsUnique().HasFilter("[code] IS NOT NULL").HasDatabaseName("UX_services_code");
        b.HasIndex(x => x.SpecialtyId);
    }
}

public class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
{
    public void Configure(EntityTypeBuilder<Invoice> b)
    {
        b.ToTable("invoices", "billing", t =>
        {
            t.HasCheckConstraint("CK_invoices_total_non_negative", "total_amount >= 0");
            t.HasCheckConstraint("CK_invoices_discount_non_negative", "discount_amount >= 0");
            t.HasCheckConstraint("CK_invoices_tax_non_negative", "tax_amount >= 0");
            t.HasCheckConstraint("CK_invoices_insurance_non_negative", "insurance_amount >= 0");
            t.HasCheckConstraint("CK_invoices_paid_non_negative", "paid_amount >= 0");
            t.HasCheckConstraint("CK_invoices_status", "status BETWEEN 0 AND 2");
        });
        b.Property(x => x.InvoiceNo).HasColumnType("varchar(30)");
        b.Property(x => x.PatientName).HasColumnType("nvarchar(255)");
        b.Property(x => x.CancelReason).HasColumnType("nvarchar(500)");
        b.HasOne(x => x.Patient).WithMany().HasForeignKey(x => x.PatientId);
        b.HasOne(x => x.Appointment).WithMany().HasForeignKey(x => x.AppointmentId);
        b.HasOne(x => x.MedicalRecord).WithMany().HasForeignKey(x => x.MedicalRecordId);
        b.HasOne(x => x.CreatedByUser).WithMany().HasForeignKey(x => x.CreatedBy);
        b.HasIndex(x => x.PatientId);
        b.HasIndex(x => x.AppointmentId);
        b.HasIndex(x => new { x.Status, x.CreatedAt }).HasDatabaseName("IX_invoices_status_created_at");
        b.HasIndex(x => x.InvoiceNo).IsUnique().HasFilter("[invoice_no] IS NOT NULL").HasDatabaseName("UX_invoices_invoice_no");
    }
}

public class InvoiceItemConfiguration : IEntityTypeConfiguration<InvoiceItem>
{
    public void Configure(EntityTypeBuilder<InvoiceItem> b)
    {
        b.ToTable("invoice_items", "billing", t =>
        {
            t.HasCheckConstraint("CK_invoice_items_exactly_one_ref",
                "(CASE WHEN medical_record_service_id IS NOT NULL THEN 1 ELSE 0 END) + (CASE WHEN medicine_id IS NOT NULL THEN 1 ELSE 0 END) = 1");
            t.HasCheckConstraint("CK_invoice_items_quantity", "quantity > 0");
            t.HasCheckConstraint("CK_invoice_items_amount", "amount >= 0");
            t.HasCheckConstraint("CK_invoice_items_discount", "discount_amount >= 0");
        });
        b.Property(x => x.Description).HasColumnType("nvarchar(255)");
        b.HasOne(x => x.Invoice).WithMany(i => i.Items).HasForeignKey(x => x.InvoiceId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.MedicalRecordService).WithMany().HasForeignKey(x => x.MedicalRecordServiceId);
        b.HasOne(x => x.Medicine).WithMany().HasForeignKey(x => x.MedicineId);
        b.HasIndex(x => x.InvoiceId);
    }
}

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> b)
    {
        b.ToTable("payments", "billing", t =>
        {
            // db.sql had "amount > 0", which contradicts VC-11 (refund = negative row); sign must match is_refund
            t.HasCheckConstraint("CK_payments_amount_sign", "(is_refund = 0 AND amount > 0) OR (is_refund = 1 AND amount < 0)");
            t.HasCheckConstraint("CK_payments_method", "method BETWEEN 0 AND 3");
        });
        b.Property(x => x.ReferenceCode).HasColumnType("varchar(100)");
        b.Property(x => x.Note).HasColumnType("nvarchar(255)");
        b.HasOne(x => x.Invoice).WithMany(i => i.Payments).HasForeignKey(x => x.InvoiceId);
        b.HasOne(x => x.ReceivedByUser).WithMany().HasForeignKey(x => x.ReceivedBy);
        b.HasIndex(x => x.InvoiceId);
    }
}

public class InsurancePolicyConfiguration : IEntityTypeConfiguration<InsurancePolicy>
{
    public void Configure(EntityTypeBuilder<InsurancePolicy> b)
    {
        b.ToTable("insurance_policies", "billing", t => t.HasCheckConstraint("CK_insurance_policies_coverage", "coverage_percent IS NULL OR coverage_percent BETWEEN 0 AND 100"));
        b.Property(x => x.PolicyNumber).HasColumnType("varchar(50)");
        b.Property(x => x.Provider).HasColumnType("nvarchar(255)");
        b.Property(x => x.CoveragePercent).HasPrecision(5, 2);
        b.HasOne(x => x.Patient).WithMany(p => p.InsurancePolicies).HasForeignKey(x => x.PatientId);
        b.HasIndex(x => x.PolicyNumber).IsUnique().HasDatabaseName("UX_insurance_policies_policy_number");
        b.HasIndex(x => x.PatientId);
    }
}
