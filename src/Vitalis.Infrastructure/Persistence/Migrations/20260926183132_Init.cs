using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Vitalis.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "scheduling");

            migrationBuilder.EnsureSchema(
                name: "clinical");

            migrationBuilder.EnsureSchema(
                name: "auth");

            migrationBuilder.EnsureSchema(
                name: "billing");

            migrationBuilder.CreateTable(
                name: "medicines",
                schema: "clinical",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    code = table.Column<string>(type: "varchar(30)", nullable: true),
                    name = table.Column<string>(type: "nvarchar(255)", nullable: false),
                    active_ingredient = table.Column<string>(type: "nvarchar(255)", nullable: true),
                    concentration = table.Column<string>(type: "nvarchar(100)", nullable: true),
                    unit = table.Column<string>(type: "nvarchar(50)", nullable: true),
                    price = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    cost_price = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: true),
                    stock_quantity = table.Column<int>(type: "int", nullable: false),
                    min_stock = table.Column<int>(type: "int", nullable: true),
                    description = table.Column<string>(type: "nvarchar(500)", nullable: true),
                    is_active = table.Column<bool>(type: "bit", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_medicines", x => x.id);
                    table.CheckConstraint("CK_medicines_price", "price >= 0");
                    table.CheckConstraint("CK_medicines_stock_non_negative", "stock_quantity >= 0");
                });

            migrationBuilder.CreateTable(
                name: "permissions",
                schema: "auth",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "varchar(100)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_permissions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "roles",
                schema: "auth",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "varchar(50)", nullable: false),
                    description = table.Column<string>(type: "nvarchar(255)", nullable: true),
                    is_system = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_roles", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "specialties",
                schema: "scheduling",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    code = table.Column<string>(type: "varchar(20)", nullable: true),
                    name = table.Column<string>(type: "nvarchar(255)", nullable: false),
                    description = table.Column<string>(type: "nvarchar(500)", nullable: true),
                    is_active = table.Column<bool>(type: "bit", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_specialties", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                schema: "auth",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    username = table.Column<string>(type: "varchar(100)", nullable: false),
                    password_hash = table.Column<string>(type: "varchar(255)", nullable: false),
                    email = table.Column<string>(type: "varchar(255)", nullable: true),
                    full_name = table.Column<string>(type: "nvarchar(255)", nullable: true),
                    phone = table.Column<string>(type: "varchar(20)", nullable: true),
                    is_active = table.Column<bool>(type: "bit", nullable: false),
                    is_deleted = table.Column<bool>(type: "bit", nullable: false),
                    email_confirmed = table.Column<bool>(type: "bit", nullable: false),
                    avatar_url = table.Column<string>(type: "varchar(500)", nullable: true),
                    last_login_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    failed_login_count = table.Column<int>(type: "int", nullable: false),
                    lockout_end = table.Column<DateTime>(type: "datetime2", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.id);
                    table.CheckConstraint("CK_users_failed_login_count", "failed_login_count >= 0");
                });

            migrationBuilder.CreateTable(
                name: "medicine_batches",
                schema: "clinical",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    medicine_id = table.Column<int>(type: "int", nullable: false),
                    batch_no = table.Column<string>(type: "varchar(50)", nullable: false),
                    expiry_date = table.Column<DateOnly>(type: "date", nullable: false),
                    quantity = table.Column<int>(type: "int", nullable: false),
                    import_price = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_medicine_batches", x => x.id);
                    table.CheckConstraint("CK_medicine_batches_quantity", "quantity >= 0");
                    table.ForeignKey(
                        name: "FK_medicine_batches_medicines_medicine_id",
                        column: x => x.medicine_id,
                        principalSchema: "clinical",
                        principalTable: "medicines",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "role_permissions",
                schema: "auth",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    role_id = table.Column<int>(type: "int", nullable: false),
                    permission_id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_role_permissions", x => x.id);
                    table.ForeignKey(
                        name: "FK_role_permissions_permissions_permission_id",
                        column: x => x.permission_id,
                        principalSchema: "auth",
                        principalTable: "permissions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_role_permissions_roles_role_id",
                        column: x => x.role_id,
                        principalSchema: "auth",
                        principalTable: "roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "services",
                schema: "billing",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    code = table.Column<string>(type: "varchar(20)", nullable: true),
                    specialty_id = table.Column<int>(type: "int", nullable: true),
                    name = table.Column<string>(type: "nvarchar(255)", nullable: false),
                    description = table.Column<string>(type: "nvarchar(500)", nullable: true),
                    price = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    duration_minutes = table.Column<int>(type: "int", nullable: true),
                    is_active = table.Column<bool>(type: "bit", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_services", x => x.id);
                    table.CheckConstraint("CK_services_price", "price >= 0");
                    table.ForeignKey(
                        name: "FK_services_specialties_specialty_id",
                        column: x => x.specialty_id,
                        principalSchema: "scheduling",
                        principalTable: "specialties",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "audit_logs",
                schema: "auth",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<int>(type: "int", nullable: true),
                    action = table.Column<string>(type: "varchar(50)", nullable: false),
                    entity_name = table.Column<string>(type: "varchar(100)", nullable: false),
                    entity_id = table.Column<int>(type: "int", nullable: true),
                    detail = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_audit_logs", x => x.id);
                    table.ForeignKey(
                        name: "FK_audit_logs_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "auth",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "doctors",
                schema: "scheduling",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    specialty_id = table.Column<int>(type: "int", nullable: false),
                    full_name = table.Column<string>(type: "nvarchar(255)", nullable: false),
                    title = table.Column<string>(type: "nvarchar(50)", nullable: true),
                    license_number = table.Column<string>(type: "varchar(50)", nullable: true),
                    phone = table.Column<string>(type: "varchar(20)", nullable: true),
                    email = table.Column<string>(type: "varchar(255)", nullable: true),
                    room = table.Column<string>(type: "varchar(50)", nullable: true),
                    consultation_fee = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: true),
                    bio = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    avatar_url = table.Column<string>(type: "varchar(500)", nullable: true),
                    experience_years = table.Column<int>(type: "int", nullable: true),
                    max_patients_per_day = table.Column<int>(type: "int", nullable: true),
                    is_active = table.Column<bool>(type: "bit", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_doctors", x => x.id);
                    table.CheckConstraint("CK_doctors_consultation_fee", "consultation_fee IS NULL OR consultation_fee >= 0");
                    table.ForeignKey(
                        name: "FK_doctors_specialties_specialty_id",
                        column: x => x.specialty_id,
                        principalSchema: "scheduling",
                        principalTable: "specialties",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_doctors_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "auth",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "patients",
                schema: "scheduling",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<int>(type: "int", nullable: true),
                    patient_code = table.Column<string>(type: "varchar(20)", nullable: true),
                    full_name = table.Column<string>(type: "nvarchar(255)", nullable: false),
                    gender = table.Column<byte>(type: "tinyint", nullable: true),
                    date_of_birth = table.Column<DateOnly>(type: "date", nullable: true),
                    phone = table.Column<string>(type: "varchar(20)", nullable: true),
                    email = table.Column<string>(type: "varchar(255)", nullable: true),
                    address = table.Column<string>(type: "nvarchar(500)", nullable: true),
                    national_id = table.Column<string>(type: "varchar(20)", nullable: true),
                    insurance_number = table.Column<string>(type: "varchar(20)", nullable: true),
                    blood_type = table.Column<string>(type: "varchar(5)", nullable: true),
                    emergency_contact_name = table.Column<string>(type: "nvarchar(255)", nullable: true),
                    emergency_contact_phone = table.Column<string>(type: "varchar(20)", nullable: true),
                    is_active = table.Column<bool>(type: "bit", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_patients", x => x.id);
                    table.CheckConstraint("CK_patients_gender", "gender BETWEEN 0 AND 2");
                    table.ForeignKey(
                        name: "FK_patients_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "auth",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "refresh_tokens",
                schema: "auth",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    token_hash = table.Column<string>(type: "char(64)", nullable: false),
                    device_info = table.Column<string>(type: "nvarchar(255)", nullable: true),
                    ip_address = table.Column<string>(type: "varchar(45)", nullable: true),
                    replaced_by_token_hash = table.Column<string>(type: "char(64)", nullable: true),
                    expires_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    revoked_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_refresh_tokens", x => x.id);
                    table.ForeignKey(
                        name: "FK_refresh_tokens_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "auth",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_roles",
                schema: "auth",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    role_id = table.Column<int>(type: "int", nullable: false),
                    assigned_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    assigned_by = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_roles", x => x.id);
                    table.ForeignKey(
                        name: "FK_user_roles_roles_role_id",
                        column: x => x.role_id,
                        principalSchema: "auth",
                        principalTable: "roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_user_roles_users_assigned_by",
                        column: x => x.assigned_by,
                        principalSchema: "auth",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_user_roles_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "auth",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "doctor_schedules",
                schema: "scheduling",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    doctor_id = table.Column<int>(type: "int", nullable: false),
                    day_of_week = table.Column<byte>(type: "tinyint", nullable: false),
                    start_time = table.Column<TimeOnly>(type: "time", nullable: false),
                    end_time = table.Column<TimeOnly>(type: "time", nullable: false),
                    break_start = table.Column<TimeOnly>(type: "time", nullable: true),
                    break_end = table.Column<TimeOnly>(type: "time", nullable: true),
                    slot_minutes = table.Column<int>(type: "int", nullable: false),
                    effective_from = table.Column<DateOnly>(type: "date", nullable: false),
                    effective_to = table.Column<DateOnly>(type: "date", nullable: true),
                    is_active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_doctor_schedules", x => x.id);
                    table.CheckConstraint("CK_doctor_schedules_break", "(break_start IS NULL AND break_end IS NULL) OR (break_end > break_start)");
                    table.CheckConstraint("CK_doctor_schedules_day_of_week", "day_of_week BETWEEN 0 AND 6");
                    table.CheckConstraint("CK_doctor_schedules_effective", "effective_to IS NULL OR effective_to > effective_from");
                    table.CheckConstraint("CK_doctor_schedules_time", "end_time > start_time");
                    table.ForeignKey(
                        name: "FK_doctor_schedules_doctors_doctor_id",
                        column: x => x.doctor_id,
                        principalSchema: "scheduling",
                        principalTable: "doctors",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "doctor_time_off",
                schema: "scheduling",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    doctor_id = table.Column<int>(type: "int", nullable: true),
                    type = table.Column<byte>(type: "tinyint", nullable: false),
                    is_full_day = table.Column<bool>(type: "bit", nullable: false),
                    start_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    end_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    reason = table.Column<string>(type: "nvarchar(255)", nullable: true),
                    approved_by = table.Column<int>(type: "int", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_doctor_time_off", x => x.id);
                    table.CheckConstraint("CK_doctor_time_off_time", "end_at > start_at");
                    table.CheckConstraint("CK_doctor_time_off_type", "type BETWEEN 0 AND 2");
                    table.ForeignKey(
                        name: "FK_doctor_time_off_doctors_doctor_id",
                        column: x => x.doctor_id,
                        principalSchema: "scheduling",
                        principalTable: "doctors",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_doctor_time_off_users_approved_by",
                        column: x => x.approved_by,
                        principalSchema: "auth",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "appointments",
                schema: "scheduling",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    appointment_code = table.Column<string>(type: "varchar(20)", nullable: true),
                    patient_id = table.Column<int>(type: "int", nullable: false),
                    doctor_id = table.Column<int>(type: "int", nullable: false),
                    start_time = table.Column<DateTime>(type: "datetime2", nullable: false),
                    end_time = table.Column<DateTime>(type: "datetime2", nullable: false),
                    status = table.Column<byte>(type: "tinyint", nullable: false),
                    source = table.Column<byte>(type: "tinyint", nullable: false),
                    queue_number = table.Column<int>(type: "int", nullable: true),
                    fee_snapshot = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: true),
                    reason = table.Column<string>(type: "nvarchar(500)", nullable: true),
                    cancel_reason = table.Column<string>(type: "nvarchar(500)", nullable: true),
                    note = table.Column<string>(type: "nvarchar(500)", nullable: true),
                    checked_in_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    completed_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    created_by = table.Column<int>(type: "int", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_appointments", x => x.id);
                    table.CheckConstraint("CK_appointments_fee_snapshot", "fee_snapshot IS NULL OR fee_snapshot >= 0");
                    table.CheckConstraint("CK_appointments_source", "source BETWEEN 0 AND 2");
                    table.CheckConstraint("CK_appointments_status", "status BETWEEN 0 AND 6");
                    table.CheckConstraint("CK_appointments_time", "end_time > start_time");
                    table.ForeignKey(
                        name: "FK_appointments_doctors_doctor_id",
                        column: x => x.doctor_id,
                        principalSchema: "scheduling",
                        principalTable: "doctors",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_appointments_patients_patient_id",
                        column: x => x.patient_id,
                        principalSchema: "scheduling",
                        principalTable: "patients",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_appointments_users_created_by",
                        column: x => x.created_by,
                        principalSchema: "auth",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "insurance_policies",
                schema: "billing",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    patient_id = table.Column<int>(type: "int", nullable: false),
                    policy_number = table.Column<string>(type: "varchar(50)", nullable: false),
                    provider = table.Column<string>(type: "nvarchar(255)", nullable: true),
                    coverage_percent = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    valid_from = table.Column<DateOnly>(type: "date", nullable: true),
                    valid_to = table.Column<DateOnly>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_insurance_policies", x => x.id);
                    table.CheckConstraint("CK_insurance_policies_coverage", "coverage_percent IS NULL OR coverage_percent BETWEEN 0 AND 100");
                    table.ForeignKey(
                        name: "FK_insurance_policies_patients_patient_id",
                        column: x => x.patient_id,
                        principalSchema: "scheduling",
                        principalTable: "patients",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "patient_allergies",
                schema: "clinical",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    patient_id = table.Column<int>(type: "int", nullable: false),
                    allergen = table.Column<string>(type: "nvarchar(255)", nullable: false),
                    severity = table.Column<byte>(type: "tinyint", nullable: true),
                    note = table.Column<string>(type: "nvarchar(255)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_patient_allergies", x => x.id);
                    table.CheckConstraint("CK_patient_allergies_severity", "severity BETWEEN 0 AND 2");
                    table.ForeignKey(
                        name: "FK_patient_allergies_patients_patient_id",
                        column: x => x.patient_id,
                        principalSchema: "scheduling",
                        principalTable: "patients",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "appointment_status_history",
                schema: "scheduling",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    appointment_id = table.Column<int>(type: "int", nullable: false),
                    from_status = table.Column<byte>(type: "tinyint", nullable: true),
                    to_status = table.Column<byte>(type: "tinyint", nullable: false),
                    changed_by = table.Column<int>(type: "int", nullable: true),
                    reason = table.Column<string>(type: "nvarchar(500)", nullable: true),
                    changed_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_appointment_status_history", x => x.id);
                    table.CheckConstraint("CK_status_history_from_status", "from_status BETWEEN 0 AND 6");
                    table.CheckConstraint("CK_status_history_to_status", "to_status BETWEEN 0 AND 6");
                    table.ForeignKey(
                        name: "FK_appointment_status_history_appointments_appointment_id",
                        column: x => x.appointment_id,
                        principalSchema: "scheduling",
                        principalTable: "appointments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_appointment_status_history_users_changed_by",
                        column: x => x.changed_by,
                        principalSchema: "auth",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "medical_records",
                schema: "clinical",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    appointment_id = table.Column<int>(type: "int", nullable: false),
                    patient_id = table.Column<int>(type: "int", nullable: false),
                    doctor_id = table.Column<int>(type: "int", nullable: false),
                    symptoms = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    diagnosis = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    icd10_code = table.Column<string>(type: "varchar(10)", nullable: true),
                    treatment_plan = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    follow_up_date = table.Column<DateOnly>(type: "date", nullable: true),
                    note = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    status = table.Column<byte>(type: "tinyint", nullable: false),
                    finalized_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_medical_records", x => x.id);
                    table.CheckConstraint("CK_medical_records_status", "status BETWEEN 0 AND 1");
                    table.ForeignKey(
                        name: "FK_medical_records_appointments_appointment_id",
                        column: x => x.appointment_id,
                        principalSchema: "scheduling",
                        principalTable: "appointments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_medical_records_doctors_doctor_id",
                        column: x => x.doctor_id,
                        principalSchema: "scheduling",
                        principalTable: "doctors",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_medical_records_patients_patient_id",
                        column: x => x.patient_id,
                        principalSchema: "scheduling",
                        principalTable: "patients",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "notifications",
                schema: "scheduling",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    appointment_id = table.Column<int>(type: "int", nullable: false),
                    channel = table.Column<byte>(type: "tinyint", nullable: false),
                    status = table.Column<byte>(type: "tinyint", nullable: false),
                    sent_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_notifications", x => x.id);
                    table.CheckConstraint("CK_notifications_channel", "channel BETWEEN 0 AND 2");
                    table.CheckConstraint("CK_notifications_status", "status BETWEEN 0 AND 2");
                    table.ForeignKey(
                        name: "FK_notifications_appointments_appointment_id",
                        column: x => x.appointment_id,
                        principalSchema: "scheduling",
                        principalTable: "appointments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "attachments",
                schema: "clinical",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    medical_record_id = table.Column<int>(type: "int", nullable: false),
                    file_url = table.Column<string>(type: "nvarchar(500)", nullable: false),
                    file_type = table.Column<string>(type: "varchar(50)", nullable: true),
                    uploaded_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_attachments", x => x.id);
                    table.ForeignKey(
                        name: "FK_attachments_medical_records_medical_record_id",
                        column: x => x.medical_record_id,
                        principalSchema: "clinical",
                        principalTable: "medical_records",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "invoices",
                schema: "billing",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    invoice_no = table.Column<string>(type: "varchar(30)", nullable: true),
                    patient_id = table.Column<int>(type: "int", nullable: false),
                    appointment_id = table.Column<int>(type: "int", nullable: true),
                    medical_record_id = table.Column<int>(type: "int", nullable: true),
                    patient_name = table.Column<string>(type: "nvarchar(255)", nullable: true),
                    total_amount = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    discount_amount = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    tax_amount = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    insurance_amount = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    paid_amount = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    status = table.Column<byte>(type: "tinyint", nullable: false),
                    cancelled_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    cancel_reason = table.Column<string>(type: "nvarchar(500)", nullable: true),
                    created_by = table.Column<int>(type: "int", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_invoices", x => x.id);
                    table.CheckConstraint("CK_invoices_discount_non_negative", "discount_amount >= 0");
                    table.CheckConstraint("CK_invoices_insurance_non_negative", "insurance_amount >= 0");
                    table.CheckConstraint("CK_invoices_paid_non_negative", "paid_amount >= 0");
                    table.CheckConstraint("CK_invoices_status", "status BETWEEN 0 AND 2");
                    table.CheckConstraint("CK_invoices_tax_non_negative", "tax_amount >= 0");
                    table.CheckConstraint("CK_invoices_total_non_negative", "total_amount >= 0");
                    table.ForeignKey(
                        name: "FK_invoices_appointments_appointment_id",
                        column: x => x.appointment_id,
                        principalSchema: "scheduling",
                        principalTable: "appointments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_invoices_medical_records_medical_record_id",
                        column: x => x.medical_record_id,
                        principalSchema: "clinical",
                        principalTable: "medical_records",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_invoices_patients_patient_id",
                        column: x => x.patient_id,
                        principalSchema: "scheduling",
                        principalTable: "patients",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_invoices_users_created_by",
                        column: x => x.created_by,
                        principalSchema: "auth",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "medical_record_services",
                schema: "clinical",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    medical_record_id = table.Column<int>(type: "int", nullable: false),
                    service_id = table.Column<int>(type: "int", nullable: false),
                    quantity = table.Column<int>(type: "int", nullable: false),
                    unit_price_snapshot = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    status = table.Column<byte>(type: "tinyint", nullable: false),
                    performed_by = table.Column<int>(type: "int", nullable: true),
                    ordered_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_medical_record_services", x => x.id);
                    table.CheckConstraint("CK_mrs_quantity", "quantity > 0");
                    table.CheckConstraint("CK_mrs_status", "status BETWEEN 0 AND 3");
                    table.CheckConstraint("CK_mrs_unit_price", "unit_price_snapshot >= 0");
                    table.ForeignKey(
                        name: "FK_medical_record_services_medical_records_medical_record_id",
                        column: x => x.medical_record_id,
                        principalSchema: "clinical",
                        principalTable: "medical_records",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_medical_record_services_services_service_id",
                        column: x => x.service_id,
                        principalSchema: "billing",
                        principalTable: "services",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_medical_record_services_users_performed_by",
                        column: x => x.performed_by,
                        principalSchema: "auth",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "patient_vitals",
                schema: "clinical",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    medical_record_id = table.Column<int>(type: "int", nullable: false),
                    temperature = table.Column<decimal>(type: "decimal(4,1)", precision: 4, scale: 1, nullable: true),
                    pulse = table.Column<int>(type: "int", nullable: true),
                    blood_pressure = table.Column<string>(type: "varchar(20)", nullable: true),
                    weight = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    height = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_patient_vitals", x => x.id);
                    table.ForeignKey(
                        name: "FK_patient_vitals_medical_records_medical_record_id",
                        column: x => x.medical_record_id,
                        principalSchema: "clinical",
                        principalTable: "medical_records",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "prescriptions",
                schema: "clinical",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    medical_record_id = table.Column<int>(type: "int", nullable: false),
                    doctor_id = table.Column<int>(type: "int", nullable: false),
                    status = table.Column<byte>(type: "tinyint", nullable: false),
                    dispensed_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    dispensed_by = table.Column<int>(type: "int", nullable: true),
                    note = table.Column<string>(type: "nvarchar(500)", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_prescriptions", x => x.id);
                    table.CheckConstraint("CK_prescriptions_status", "status BETWEEN 0 AND 1");
                    table.ForeignKey(
                        name: "FK_prescriptions_doctors_doctor_id",
                        column: x => x.doctor_id,
                        principalSchema: "scheduling",
                        principalTable: "doctors",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_prescriptions_medical_records_medical_record_id",
                        column: x => x.medical_record_id,
                        principalSchema: "clinical",
                        principalTable: "medical_records",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_prescriptions_users_dispensed_by",
                        column: x => x.dispensed_by,
                        principalSchema: "auth",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "payments",
                schema: "billing",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    invoice_id = table.Column<int>(type: "int", nullable: false),
                    amount = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    method = table.Column<byte>(type: "tinyint", nullable: false),
                    reference_code = table.Column<string>(type: "varchar(100)", nullable: true),
                    received_by = table.Column<int>(type: "int", nullable: true),
                    is_refund = table.Column<bool>(type: "bit", nullable: false),
                    paid_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    note = table.Column<string>(type: "nvarchar(255)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_payments", x => x.id);
                    table.CheckConstraint("CK_payments_amount_sign", "(is_refund = 0 AND amount > 0) OR (is_refund = 1 AND amount < 0)");
                    table.CheckConstraint("CK_payments_method", "method BETWEEN 0 AND 3");
                    table.ForeignKey(
                        name: "FK_payments_invoices_invoice_id",
                        column: x => x.invoice_id,
                        principalSchema: "billing",
                        principalTable: "invoices",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_payments_users_received_by",
                        column: x => x.received_by,
                        principalSchema: "auth",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "invoice_items",
                schema: "billing",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    invoice_id = table.Column<int>(type: "int", nullable: false),
                    medical_record_service_id = table.Column<int>(type: "int", nullable: true),
                    medicine_id = table.Column<int>(type: "int", nullable: true),
                    description = table.Column<string>(type: "nvarchar(255)", nullable: true),
                    quantity = table.Column<int>(type: "int", nullable: false),
                    unit_price = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    discount_amount = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    amount = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_invoice_items", x => x.id);
                    table.CheckConstraint("CK_invoice_items_amount", "amount >= 0");
                    table.CheckConstraint("CK_invoice_items_discount", "discount_amount >= 0");
                    table.CheckConstraint("CK_invoice_items_exactly_one_ref", "(CASE WHEN medical_record_service_id IS NOT NULL THEN 1 ELSE 0 END) + (CASE WHEN medicine_id IS NOT NULL THEN 1 ELSE 0 END) = 1");
                    table.CheckConstraint("CK_invoice_items_quantity", "quantity > 0");
                    table.ForeignKey(
                        name: "FK_invoice_items_invoices_invoice_id",
                        column: x => x.invoice_id,
                        principalSchema: "billing",
                        principalTable: "invoices",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_invoice_items_medical_record_services_medical_record_service_id",
                        column: x => x.medical_record_service_id,
                        principalSchema: "clinical",
                        principalTable: "medical_record_services",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_invoice_items_medicines_medicine_id",
                        column: x => x.medicine_id,
                        principalSchema: "clinical",
                        principalTable: "medicines",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "lab_results",
                schema: "clinical",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    medical_record_service_id = table.Column<int>(type: "int", nullable: false),
                    result_value = table.Column<string>(type: "nvarchar(500)", nullable: true),
                    reference_range = table.Column<string>(type: "nvarchar(255)", nullable: true),
                    conclusion = table.Column<string>(type: "nvarchar(500)", nullable: true),
                    resulted_at = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_lab_results", x => x.id);
                    table.ForeignKey(
                        name: "FK_lab_results_medical_record_services_medical_record_service_id",
                        column: x => x.medical_record_service_id,
                        principalSchema: "clinical",
                        principalTable: "medical_record_services",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "medicine_stock_transactions",
                schema: "clinical",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    medicine_id = table.Column<int>(type: "int", nullable: false),
                    batch_id = table.Column<int>(type: "int", nullable: false),
                    type = table.Column<byte>(type: "tinyint", nullable: false),
                    quantity = table.Column<int>(type: "int", nullable: false),
                    prescription_id = table.Column<int>(type: "int", nullable: true),
                    created_by = table.Column<int>(type: "int", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_medicine_stock_transactions", x => x.id);
                    table.CheckConstraint("CK_mst_quantity", "quantity > 0");
                    table.CheckConstraint("CK_mst_type", "type BETWEEN 0 AND 2");
                    table.ForeignKey(
                        name: "FK_medicine_stock_transactions_medicine_batches_batch_id",
                        column: x => x.batch_id,
                        principalSchema: "clinical",
                        principalTable: "medicine_batches",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_medicine_stock_transactions_medicines_medicine_id",
                        column: x => x.medicine_id,
                        principalSchema: "clinical",
                        principalTable: "medicines",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_medicine_stock_transactions_prescriptions_prescription_id",
                        column: x => x.prescription_id,
                        principalSchema: "clinical",
                        principalTable: "prescriptions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_medicine_stock_transactions_users_created_by",
                        column: x => x.created_by,
                        principalSchema: "auth",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "prescription_items",
                schema: "clinical",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    prescription_id = table.Column<int>(type: "int", nullable: false),
                    medicine_id = table.Column<int>(type: "int", nullable: false),
                    medicine_name_snapshot = table.Column<string>(type: "nvarchar(255)", nullable: true),
                    unit_price_snapshot = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: true),
                    quantity = table.Column<int>(type: "int", nullable: false),
                    dosage = table.Column<string>(type: "nvarchar(255)", nullable: true),
                    duration_days = table.Column<int>(type: "int", nullable: true),
                    frequency = table.Column<string>(type: "nvarchar(50)", nullable: true),
                    instruction = table.Column<string>(type: "nvarchar(255)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_prescription_items", x => x.id);
                    table.CheckConstraint("CK_prescription_items_quantity", "quantity > 0");
                    table.ForeignKey(
                        name: "FK_prescription_items_medicines_medicine_id",
                        column: x => x.medicine_id,
                        principalSchema: "clinical",
                        principalTable: "medicines",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_prescription_items_prescriptions_prescription_id",
                        column: x => x.prescription_id,
                        principalSchema: "clinical",
                        principalTable: "prescriptions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_appointment_status_history_changed_by",
                schema: "scheduling",
                table: "appointment_status_history",
                column: "changed_by");

            migrationBuilder.CreateIndex(
                name: "IX_status_history_appointment_id",
                schema: "scheduling",
                table: "appointment_status_history",
                column: "appointment_id");

            migrationBuilder.CreateIndex(
                name: "IX_appointments_created_by",
                schema: "scheduling",
                table: "appointments",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_appointments_patient_id",
                schema: "scheduling",
                table: "appointments",
                column: "patient_id");

            migrationBuilder.CreateIndex(
                name: "IX_appointments_start_time",
                schema: "scheduling",
                table: "appointments",
                column: "start_time");

            migrationBuilder.CreateIndex(
                name: "UX_appointments_code",
                schema: "scheduling",
                table: "appointments",
                column: "appointment_code",
                unique: true,
                filter: "[appointment_code] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "UX_appointments_doctor_slot",
                schema: "scheduling",
                table: "appointments",
                columns: new[] { "doctor_id", "start_time" },
                unique: true,
                filter: "[status] < 5");

            migrationBuilder.CreateIndex(
                name: "IX_attachments_medical_record_id",
                schema: "clinical",
                table: "attachments",
                column: "medical_record_id");

            migrationBuilder.CreateIndex(
                name: "IX_audit_logs_entity",
                schema: "auth",
                table: "audit_logs",
                columns: new[] { "entity_name", "entity_id" });

            migrationBuilder.CreateIndex(
                name: "IX_audit_logs_user_id",
                schema: "auth",
                table: "audit_logs",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_doctor_schedules_doctor_id",
                schema: "scheduling",
                table: "doctor_schedules",
                column: "doctor_id");

            migrationBuilder.CreateIndex(
                name: "UX_doctor_schedules_slot",
                schema: "scheduling",
                table: "doctor_schedules",
                columns: new[] { "doctor_id", "day_of_week", "start_time" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_doctor_time_off_approved_by",
                schema: "scheduling",
                table: "doctor_time_off",
                column: "approved_by");

            migrationBuilder.CreateIndex(
                name: "IX_doctor_time_off_doctor_start",
                schema: "scheduling",
                table: "doctor_time_off",
                columns: new[] { "doctor_id", "start_at" });

            migrationBuilder.CreateIndex(
                name: "IX_doctors_specialty_id",
                schema: "scheduling",
                table: "doctors",
                column: "specialty_id");

            migrationBuilder.CreateIndex(
                name: "IX_doctors_user_id",
                schema: "scheduling",
                table: "doctors",
                column: "user_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_doctors_license_number",
                schema: "scheduling",
                table: "doctors",
                column: "license_number",
                unique: true,
                filter: "[license_number] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_insurance_policies_patient_id",
                schema: "billing",
                table: "insurance_policies",
                column: "patient_id");

            migrationBuilder.CreateIndex(
                name: "UX_insurance_policies_policy_number",
                schema: "billing",
                table: "insurance_policies",
                column: "policy_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_invoice_items_invoice_id",
                schema: "billing",
                table: "invoice_items",
                column: "invoice_id");

            migrationBuilder.CreateIndex(
                name: "IX_invoice_items_medical_record_service_id",
                schema: "billing",
                table: "invoice_items",
                column: "medical_record_service_id");

            migrationBuilder.CreateIndex(
                name: "IX_invoice_items_medicine_id",
                schema: "billing",
                table: "invoice_items",
                column: "medicine_id");

            migrationBuilder.CreateIndex(
                name: "IX_invoices_appointment_id",
                schema: "billing",
                table: "invoices",
                column: "appointment_id");

            migrationBuilder.CreateIndex(
                name: "IX_invoices_created_by",
                schema: "billing",
                table: "invoices",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_invoices_medical_record_id",
                schema: "billing",
                table: "invoices",
                column: "medical_record_id");

            migrationBuilder.CreateIndex(
                name: "IX_invoices_patient_id",
                schema: "billing",
                table: "invoices",
                column: "patient_id");

            migrationBuilder.CreateIndex(
                name: "IX_invoices_status_created_at",
                schema: "billing",
                table: "invoices",
                columns: new[] { "status", "created_at" });

            migrationBuilder.CreateIndex(
                name: "UX_invoices_invoice_no",
                schema: "billing",
                table: "invoices",
                column: "invoice_no",
                unique: true,
                filter: "[invoice_no] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_lab_results_mrs_id",
                schema: "clinical",
                table: "lab_results",
                column: "medical_record_service_id");

            migrationBuilder.CreateIndex(
                name: "IX_medical_record_services_performed_by",
                schema: "clinical",
                table: "medical_record_services",
                column: "performed_by");

            migrationBuilder.CreateIndex(
                name: "IX_medical_record_services_service_id",
                schema: "clinical",
                table: "medical_record_services",
                column: "service_id");

            migrationBuilder.CreateIndex(
                name: "IX_mrs_medical_record_status",
                schema: "clinical",
                table: "medical_record_services",
                columns: new[] { "medical_record_id", "status" });

            migrationBuilder.CreateIndex(
                name: "IX_medical_records_appointment_id",
                schema: "clinical",
                table: "medical_records",
                column: "appointment_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_medical_records_doctor_id",
                schema: "clinical",
                table: "medical_records",
                column: "doctor_id");

            migrationBuilder.CreateIndex(
                name: "IX_medical_records_patient_id",
                schema: "clinical",
                table: "medical_records",
                column: "patient_id");

            migrationBuilder.CreateIndex(
                name: "IX_medicine_batches_expiry_date",
                schema: "clinical",
                table: "medicine_batches",
                column: "expiry_date");

            migrationBuilder.CreateIndex(
                name: "UX_medicine_batches_medicine_batch_no",
                schema: "clinical",
                table: "medicine_batches",
                columns: new[] { "medicine_id", "batch_no" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_medicine_stock_transactions_batch_id",
                schema: "clinical",
                table: "medicine_stock_transactions",
                column: "batch_id");

            migrationBuilder.CreateIndex(
                name: "IX_medicine_stock_transactions_created_by",
                schema: "clinical",
                table: "medicine_stock_transactions",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_medicine_stock_transactions_medicine_id",
                schema: "clinical",
                table: "medicine_stock_transactions",
                column: "medicine_id");

            migrationBuilder.CreateIndex(
                name: "IX_medicine_stock_transactions_prescription_id",
                schema: "clinical",
                table: "medicine_stock_transactions",
                column: "prescription_id");

            migrationBuilder.CreateIndex(
                name: "UX_medicines_code",
                schema: "clinical",
                table: "medicines",
                column: "code",
                unique: true,
                filter: "[code] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_notifications_appointment_id",
                schema: "scheduling",
                table: "notifications",
                column: "appointment_id");

            migrationBuilder.CreateIndex(
                name: "IX_patient_allergies_patient_id",
                schema: "clinical",
                table: "patient_allergies",
                column: "patient_id");

            migrationBuilder.CreateIndex(
                name: "IX_patient_vitals_medical_record_id",
                schema: "clinical",
                table: "patient_vitals",
                column: "medical_record_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_patients_full_name",
                schema: "scheduling",
                table: "patients",
                column: "full_name");

            migrationBuilder.CreateIndex(
                name: "IX_patients_phone",
                schema: "scheduling",
                table: "patients",
                column: "phone");

            migrationBuilder.CreateIndex(
                name: "UX_patients_patient_code",
                schema: "scheduling",
                table: "patients",
                column: "patient_code",
                unique: true,
                filter: "[patient_code] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "UX_patients_user_id",
                schema: "scheduling",
                table: "patients",
                column: "user_id",
                unique: true,
                filter: "[user_id] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_payments_invoice_id",
                schema: "billing",
                table: "payments",
                column: "invoice_id");

            migrationBuilder.CreateIndex(
                name: "IX_payments_received_by",
                schema: "billing",
                table: "payments",
                column: "received_by");

            migrationBuilder.CreateIndex(
                name: "IX_permissions_name",
                schema: "auth",
                table: "permissions",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_prescription_items_medicine_id",
                schema: "clinical",
                table: "prescription_items",
                column: "medicine_id");

            migrationBuilder.CreateIndex(
                name: "IX_prescription_items_prescription_id",
                schema: "clinical",
                table: "prescription_items",
                column: "prescription_id");

            migrationBuilder.CreateIndex(
                name: "IX_prescriptions_dispensed_by",
                schema: "clinical",
                table: "prescriptions",
                column: "dispensed_by");

            migrationBuilder.CreateIndex(
                name: "IX_prescriptions_doctor_id",
                schema: "clinical",
                table: "prescriptions",
                column: "doctor_id");

            migrationBuilder.CreateIndex(
                name: "IX_prescriptions_medical_record_id",
                schema: "clinical",
                table: "prescriptions",
                column: "medical_record_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_refresh_tokens_user_id",
                schema: "auth",
                table: "refresh_tokens",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "UX_refresh_tokens_token_hash",
                schema: "auth",
                table: "refresh_tokens",
                column: "token_hash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_role_permissions_permission_id",
                schema: "auth",
                table: "role_permissions",
                column: "permission_id");

            migrationBuilder.CreateIndex(
                name: "UQ_role_permissions",
                schema: "auth",
                table: "role_permissions",
                columns: new[] { "role_id", "permission_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_roles_name",
                schema: "auth",
                table: "roles",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_services_specialty_id",
                schema: "billing",
                table: "services",
                column: "specialty_id");

            migrationBuilder.CreateIndex(
                name: "UX_services_code",
                schema: "billing",
                table: "services",
                column: "code",
                unique: true,
                filter: "[code] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "UX_specialties_code",
                schema: "scheduling",
                table: "specialties",
                column: "code",
                unique: true,
                filter: "[code] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_user_roles_assigned_by",
                schema: "auth",
                table: "user_roles",
                column: "assigned_by");

            migrationBuilder.CreateIndex(
                name: "IX_user_roles_role_id",
                schema: "auth",
                table: "user_roles",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "UQ_user_roles",
                schema: "auth",
                table: "user_roles",
                columns: new[] { "user_id", "role_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_users_username",
                schema: "auth",
                table: "users",
                column: "username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_users_email",
                schema: "auth",
                table: "users",
                column: "email",
                unique: true,
                filter: "[email] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "appointment_status_history",
                schema: "scheduling");

            migrationBuilder.DropTable(
                name: "attachments",
                schema: "clinical");

            migrationBuilder.DropTable(
                name: "audit_logs",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "doctor_schedules",
                schema: "scheduling");

            migrationBuilder.DropTable(
                name: "doctor_time_off",
                schema: "scheduling");

            migrationBuilder.DropTable(
                name: "insurance_policies",
                schema: "billing");

            migrationBuilder.DropTable(
                name: "invoice_items",
                schema: "billing");

            migrationBuilder.DropTable(
                name: "lab_results",
                schema: "clinical");

            migrationBuilder.DropTable(
                name: "medicine_stock_transactions",
                schema: "clinical");

            migrationBuilder.DropTable(
                name: "notifications",
                schema: "scheduling");

            migrationBuilder.DropTable(
                name: "patient_allergies",
                schema: "clinical");

            migrationBuilder.DropTable(
                name: "patient_vitals",
                schema: "clinical");

            migrationBuilder.DropTable(
                name: "payments",
                schema: "billing");

            migrationBuilder.DropTable(
                name: "prescription_items",
                schema: "clinical");

            migrationBuilder.DropTable(
                name: "refresh_tokens",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "role_permissions",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "user_roles",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "medical_record_services",
                schema: "clinical");

            migrationBuilder.DropTable(
                name: "medicine_batches",
                schema: "clinical");

            migrationBuilder.DropTable(
                name: "invoices",
                schema: "billing");

            migrationBuilder.DropTable(
                name: "prescriptions",
                schema: "clinical");

            migrationBuilder.DropTable(
                name: "permissions",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "roles",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "services",
                schema: "billing");

            migrationBuilder.DropTable(
                name: "medicines",
                schema: "clinical");

            migrationBuilder.DropTable(
                name: "medical_records",
                schema: "clinical");

            migrationBuilder.DropTable(
                name: "appointments",
                schema: "scheduling");

            migrationBuilder.DropTable(
                name: "doctors",
                schema: "scheduling");

            migrationBuilder.DropTable(
                name: "patients",
                schema: "scheduling");

            migrationBuilder.DropTable(
                name: "specialties",
                schema: "scheduling");

            migrationBuilder.DropTable(
                name: "users",
                schema: "auth");
        }
    }
}
