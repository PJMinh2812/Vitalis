using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Vitalis.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RelaxInvoiceItemsRefConstraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_invoice_items_exactly_one_ref",
                schema: "billing",
                table: "invoice_items");

            migrationBuilder.AddCheckConstraint(
                name: "CK_invoice_items_at_most_one_ref",
                schema: "billing",
                table: "invoice_items",
                sql: "(CASE WHEN medical_record_service_id IS NOT NULL THEN 1 ELSE 0 END) + (CASE WHEN medicine_id IS NOT NULL THEN 1 ELSE 0 END) <= 1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_invoice_items_at_most_one_ref",
                schema: "billing",
                table: "invoice_items");

            migrationBuilder.AddCheckConstraint(
                name: "CK_invoice_items_exactly_one_ref",
                schema: "billing",
                table: "invoice_items",
                sql: "(CASE WHEN medical_record_service_id IS NOT NULL THEN 1 ELSE 0 END) + (CASE WHEN medicine_id IS NOT NULL THEN 1 ELSE 0 END) = 1");
        }
    }
}
