using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProductionAnalysis.Data.Migrations
{
    /// <inheritdoc />
    public partial class snakeCaseMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                table: "AspNetRoleClaims");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                table: "AspNetUserClaims");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                table: "AspNetUserLogins");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                table: "AspNetUserRoles");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                table: "AspNetUserRoles");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                table: "AspNetUserTokens");

            migrationBuilder.DropForeignKey(
                name: "FK_departments_enterprises_EnterpriseId",
                table: "departments");

            migrationBuilder.DropForeignKey(
                name: "FK_employees_AspNetUsers_UserId",
                table: "employees");

            migrationBuilder.DropForeignKey(
                name: "FK_employees_departments_DepartmentId",
                table: "employees");

            migrationBuilder.DropForeignKey(
                name: "FK_employees_positions_PositionId",
                table: "employees");

            migrationBuilder.DropForeignKey(
                name: "FK_form_row_values_form_rows_FormId_FormRowOrder",
                table: "form_row_values");

            migrationBuilder.DropForeignKey(
                name: "FK_form_row_values_indicators_IndicatorId",
                table: "form_row_values");

            migrationBuilder.DropForeignKey(
                name: "FK_form_rows_forms_FormId",
                table: "form_rows");

            migrationBuilder.DropForeignKey(
                name: "FK_forms_AspNetUsers_CreatorId",
                table: "forms");

            migrationBuilder.DropForeignKey(
                name: "FK_forms_AspNetUsers_LastEditorId",
                table: "forms");

            migrationBuilder.DropForeignKey(
                name: "FK_forms_departments_DepartmentId",
                table: "forms");

            migrationBuilder.DropForeignKey(
                name: "FK_forms_employees_ExecutorId",
                table: "forms");

            migrationBuilder.DropForeignKey(
                name: "FK_forms_shifts_ShiftId",
                table: "forms");

            migrationBuilder.DropForeignKey(
                name: "FK_operations_operations_BasedOperationId",
                table: "operations");

            migrationBuilder.DropForeignKey(
                name: "FK_operations_products_BasedProductId",
                table: "operations");

            migrationBuilder.DropForeignKey(
                name: "FK_products_enterprises_EnterpriseId",
                table: "products");

            migrationBuilder.DropForeignKey(
                name: "FK_shift_schedules_auxiliary_operations_AuxiliaryOperationId",
                table: "shift_schedules");

            migrationBuilder.DropForeignKey(
                name: "FK_shift_schedules_shifts_ShiftId",
                table: "shift_schedules");

            migrationBuilder.DropForeignKey(
                name: "FK_templates_indicators_indicators_IndicatorId",
                table: "templates_indicators");

            migrationBuilder.DropForeignKey(
                name: "FK_templates_indicators_templates_TemplateDboId",
                table: "templates_indicators");

            migrationBuilder.DropForeignKey(
                name: "FK_templates_indicators_templates_TemplateId",
                table: "templates_indicators");

            migrationBuilder.DropPrimaryKey(
                name: "PK_templates_indicators",
                table: "templates_indicators");

            migrationBuilder.DropPrimaryKey(
                name: "PK_templates",
                table: "templates");

            migrationBuilder.DropPrimaryKey(
                name: "PK_shifts",
                table: "shifts");

            migrationBuilder.DropPrimaryKey(
                name: "PK_shift_schedules",
                table: "shift_schedules");

            migrationBuilder.DropPrimaryKey(
                name: "PK_products",
                table: "products");

            migrationBuilder.DropPrimaryKey(
                name: "PK_positions",
                table: "positions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_operations",
                table: "operations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_indicators",
                table: "indicators");

            migrationBuilder.DropPrimaryKey(
                name: "PK_forms",
                table: "forms");

            migrationBuilder.DropPrimaryKey(
                name: "PK_form_rows",
                table: "form_rows");

            migrationBuilder.DropPrimaryKey(
                name: "PK_form_row_values",
                table: "form_row_values");

            migrationBuilder.DropPrimaryKey(
                name: "PK_enterprises",
                table: "enterprises");

            migrationBuilder.DropPrimaryKey(
                name: "PK_employees",
                table: "employees");

            migrationBuilder.DropPrimaryKey(
                name: "PK_downtime_reason_groups",
                table: "downtime_reason_groups");

            migrationBuilder.DropPrimaryKey(
                name: "PK_departments",
                table: "departments");

            migrationBuilder.DropPrimaryKey(
                name: "PK_auxiliary_operations",
                table: "auxiliary_operations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AspNetUserTokens",
                table: "AspNetUserTokens");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AspNetUsers",
                table: "AspNetUsers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AspNetUserRoles",
                table: "AspNetUserRoles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AspNetUserLogins",
                table: "AspNetUserLogins");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AspNetUserClaims",
                table: "AspNetUserClaims");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AspNetRoles",
                table: "AspNetRoles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AspNetRoleClaims",
                table: "AspNetRoleClaims");

            migrationBuilder.RenameColumn(
                name: "Order",
                table: "templates_indicators",
                newName: "order");

            migrationBuilder.RenameColumn(
                name: "TemplateDboId",
                table: "templates_indicators",
                newName: "template_dbo_id");

            migrationBuilder.RenameColumn(
                name: "IndicatorId",
                table: "templates_indicators",
                newName: "indicator_id");

            migrationBuilder.RenameColumn(
                name: "TemplateId",
                table: "templates_indicators",
                newName: "template_id");

            migrationBuilder.RenameIndex(
                name: "IX_templates_indicators_TemplateDboId",
                table: "templates_indicators",
                newName: "ix_templates_indicators_template_dbo_id");

            migrationBuilder.RenameIndex(
                name: "IX_templates_indicators_IndicatorId",
                table: "templates_indicators",
                newName: "ix_templates_indicators_indicator_id");

            migrationBuilder.RenameColumn(
                name: "Version",
                table: "templates",
                newName: "version");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "templates",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "templates",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "PaTypeId",
                table: "templates",
                newName: "pa_type_id");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "shifts",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "shifts",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "StartTime",
                table: "shifts",
                newName: "start_time");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "shift_schedules",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "StartTime",
                table: "shift_schedules",
                newName: "start_time");

            migrationBuilder.RenameColumn(
                name: "ShiftId",
                table: "shift_schedules",
                newName: "shift_id");

            migrationBuilder.RenameColumn(
                name: "AuxiliaryOperationId",
                table: "shift_schedules",
                newName: "auxiliary_operation_id");

            migrationBuilder.RenameIndex(
                name: "IX_shift_schedules_ShiftId",
                table: "shift_schedules",
                newName: "ix_shift_schedules_shift_id");

            migrationBuilder.RenameIndex(
                name: "IX_shift_schedules_AuxiliaryOperationId",
                table: "shift_schedules",
                newName: "ix_shift_schedules_auxiliary_operation_id");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "products",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "products",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "TactTimeInSeconds",
                table: "products",
                newName: "tact_time_in_seconds");

            migrationBuilder.RenameColumn(
                name: "EnterpriseId",
                table: "products",
                newName: "enterprise_id");

            migrationBuilder.RenameIndex(
                name: "IX_products_EnterpriseId",
                table: "products",
                newName: "ix_products_enterprise_id");

            migrationBuilder.RenameColumn(
                name: "Role",
                table: "positions",
                newName: "role");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "positions",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "positions",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "operations",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "operations",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "DurationInSeconds",
                table: "operations",
                newName: "duration_in_seconds");

            migrationBuilder.RenameColumn(
                name: "BasedProductId",
                table: "operations",
                newName: "based_product_id");

            migrationBuilder.RenameColumn(
                name: "BasedOperationId",
                table: "operations",
                newName: "based_operation_id");

            migrationBuilder.RenameColumn(
                name: "BasedOnType",
                table: "operations",
                newName: "based_on_type");

            migrationBuilder.RenameIndex(
                name: "IX_operations_BasedProductId",
                table: "operations",
                newName: "ix_operations_based_product_id");

            migrationBuilder.RenameIndex(
                name: "IX_operations_BasedOperationId",
                table: "operations",
                newName: "ix_operations_based_operation_id");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "indicators",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "Formula",
                table: "indicators",
                newName: "formula");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "indicators",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "ValueType",
                table: "indicators",
                newName: "value_type");

            migrationBuilder.RenameColumn(
                name: "ValueSelector",
                table: "indicators",
                newName: "value_selector");

            migrationBuilder.RenameColumn(
                name: "InputType",
                table: "indicators",
                newName: "input_type");

            migrationBuilder.RenameColumn(
                name: "HasSummation",
                table: "indicators",
                newName: "has_summation");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "forms",
                newName: "status");

            migrationBuilder.RenameColumn(
                name: "Context",
                table: "forms",
                newName: "context");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "forms",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "UpdateDate",
                table: "forms",
                newName: "update_date");

            migrationBuilder.RenameColumn(
                name: "TotalValues",
                table: "forms",
                newName: "total_values");

            migrationBuilder.RenameColumn(
                name: "TemplateSnapshot",
                table: "forms",
                newName: "template_snapshot");

            migrationBuilder.RenameColumn(
                name: "ShiftId",
                table: "forms",
                newName: "shift_id");

            migrationBuilder.RenameColumn(
                name: "PaTypeId",
                table: "forms",
                newName: "pa_type_id");

            migrationBuilder.RenameColumn(
                name: "LastEditorId",
                table: "forms",
                newName: "last_editor_id");

            migrationBuilder.RenameColumn(
                name: "FormDate",
                table: "forms",
                newName: "form_date");

            migrationBuilder.RenameColumn(
                name: "ExecutorId",
                table: "forms",
                newName: "executor_id");

            migrationBuilder.RenameColumn(
                name: "DepartmentId",
                table: "forms",
                newName: "department_id");

            migrationBuilder.RenameColumn(
                name: "CreatorId",
                table: "forms",
                newName: "creator_id");

            migrationBuilder.RenameColumn(
                name: "CreationDate",
                table: "forms",
                newName: "creation_date");

            migrationBuilder.RenameIndex(
                name: "IX_forms_ShiftId",
                table: "forms",
                newName: "ix_forms_shift_id");

            migrationBuilder.RenameIndex(
                name: "IX_forms_LastEditorId",
                table: "forms",
                newName: "ix_forms_last_editor_id");

            migrationBuilder.RenameIndex(
                name: "IX_forms_ExecutorId",
                table: "forms",
                newName: "ix_forms_executor_id");

            migrationBuilder.RenameIndex(
                name: "IX_forms_DepartmentId",
                table: "forms",
                newName: "ix_forms_department_id");

            migrationBuilder.RenameIndex(
                name: "IX_forms_CreatorId",
                table: "forms",
                newName: "ix_forms_creator_id");

            migrationBuilder.RenameColumn(
                name: "Order",
                table: "form_rows",
                newName: "order");

            migrationBuilder.RenameColumn(
                name: "ProductId",
                table: "form_rows",
                newName: "product_id");

            migrationBuilder.RenameColumn(
                name: "IsAuxiliaryOperation",
                table: "form_rows",
                newName: "is_auxiliary_operation");

            migrationBuilder.RenameColumn(
                name: "GroupKey",
                table: "form_rows",
                newName: "group_key");

            migrationBuilder.RenameColumn(
                name: "AuxiliaryOperationId",
                table: "form_rows",
                newName: "auxiliary_operation_id");

            migrationBuilder.RenameColumn(
                name: "FormId",
                table: "form_rows",
                newName: "form_id");

            migrationBuilder.RenameIndex(
                name: "IX_form_rows_FormId",
                table: "form_rows",
                newName: "ix_form_rows_form_id");

            migrationBuilder.RenameColumn(
                name: "Value",
                table: "form_row_values",
                newName: "value");

            migrationBuilder.RenameColumn(
                name: "IndicatorId",
                table: "form_row_values",
                newName: "indicator_id");

            migrationBuilder.RenameColumn(
                name: "FormRowOrder",
                table: "form_row_values",
                newName: "form_row_order");

            migrationBuilder.RenameColumn(
                name: "FormId",
                table: "form_row_values",
                newName: "form_id");

            migrationBuilder.RenameIndex(
                name: "IX_form_row_values_IndicatorId",
                table: "form_row_values",
                newName: "ix_form_row_values_indicator_id");

            migrationBuilder.RenameIndex(
                name: "IX_form_row_values_FormId_FormRowOrder",
                table: "form_row_values",
                newName: "ix_form_row_values_form_id_form_row_order");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "enterprises",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "enterprises",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "Email",
                table: "employees",
                newName: "email");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "employees",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "employees",
                newName: "user_id");

            migrationBuilder.RenameColumn(
                name: "PositionId",
                table: "employees",
                newName: "position_id");

            migrationBuilder.RenameColumn(
                name: "MiddleName",
                table: "employees",
                newName: "middle_name");

            migrationBuilder.RenameColumn(
                name: "LastName",
                table: "employees",
                newName: "last_name");

            migrationBuilder.RenameColumn(
                name: "FirstName",
                table: "employees",
                newName: "first_name");

            migrationBuilder.RenameColumn(
                name: "DepartmentId",
                table: "employees",
                newName: "department_id");

            migrationBuilder.RenameIndex(
                name: "IX_employees_UserId",
                table: "employees",
                newName: "ix_employees_user_id");

            migrationBuilder.RenameIndex(
                name: "IX_employees_PositionId",
                table: "employees",
                newName: "ix_employees_position_id");

            migrationBuilder.RenameIndex(
                name: "IX_employees_DepartmentId",
                table: "employees",
                newName: "ix_employees_department_id");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "downtime_reason_groups",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "downtime_reason_groups",
                newName: "description");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "downtime_reason_groups",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "departments",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "departments",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "EnterpriseId",
                table: "departments",
                newName: "enterprise_id");

            migrationBuilder.RenameIndex(
                name: "IX_departments_EnterpriseId",
                table: "departments",
                newName: "ix_departments_enterprise_id");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "auxiliary_operations",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "auxiliary_operations",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "DurationInSeconds",
                table: "auxiliary_operations",
                newName: "duration_in_seconds");

            migrationBuilder.RenameColumn(
                name: "Value",
                table: "AspNetUserTokens",
                newName: "value");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "AspNetUserTokens",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "LoginProvider",
                table: "AspNetUserTokens",
                newName: "login_provider");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "AspNetUserTokens",
                newName: "user_id");

            migrationBuilder.RenameColumn(
                name: "Email",
                table: "AspNetUsers",
                newName: "email");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "AspNetUsers",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "UserName",
                table: "AspNetUsers",
                newName: "user_name");

            migrationBuilder.RenameColumn(
                name: "TwoFactorEnabled",
                table: "AspNetUsers",
                newName: "two_factor_enabled");

            migrationBuilder.RenameColumn(
                name: "SecurityStamp",
                table: "AspNetUsers",
                newName: "security_stamp");

            migrationBuilder.RenameColumn(
                name: "PhoneNumberConfirmed",
                table: "AspNetUsers",
                newName: "phone_number_confirmed");

            migrationBuilder.RenameColumn(
                name: "PhoneNumber",
                table: "AspNetUsers",
                newName: "phone_number");

            migrationBuilder.RenameColumn(
                name: "PasswordHash",
                table: "AspNetUsers",
                newName: "password_hash");

            migrationBuilder.RenameColumn(
                name: "NormalizedUserName",
                table: "AspNetUsers",
                newName: "normalized_user_name");

            migrationBuilder.RenameColumn(
                name: "NormalizedEmail",
                table: "AspNetUsers",
                newName: "normalized_email");

            migrationBuilder.RenameColumn(
                name: "MiddleName",
                table: "AspNetUsers",
                newName: "middle_name");

            migrationBuilder.RenameColumn(
                name: "LockoutEnd",
                table: "AspNetUsers",
                newName: "lockout_end");

            migrationBuilder.RenameColumn(
                name: "LockoutEnabled",
                table: "AspNetUsers",
                newName: "lockout_enabled");

            migrationBuilder.RenameColumn(
                name: "LastName",
                table: "AspNetUsers",
                newName: "last_name");

            migrationBuilder.RenameColumn(
                name: "FirstName",
                table: "AspNetUsers",
                newName: "first_name");

            migrationBuilder.RenameColumn(
                name: "EmailConfirmed",
                table: "AspNetUsers",
                newName: "email_confirmed");

            migrationBuilder.RenameColumn(
                name: "ConcurrencyStamp",
                table: "AspNetUsers",
                newName: "concurrency_stamp");

            migrationBuilder.RenameColumn(
                name: "AccessFailedCount",
                table: "AspNetUsers",
                newName: "access_failed_count");

            migrationBuilder.RenameColumn(
                name: "RoleId",
                table: "AspNetUserRoles",
                newName: "role_id");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "AspNetUserRoles",
                newName: "user_id");

            migrationBuilder.RenameIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                newName: "ix_asp_net_user_roles_role_id");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "AspNetUserLogins",
                newName: "user_id");

            migrationBuilder.RenameColumn(
                name: "ProviderDisplayName",
                table: "AspNetUserLogins",
                newName: "provider_display_name");

            migrationBuilder.RenameColumn(
                name: "ProviderKey",
                table: "AspNetUserLogins",
                newName: "provider_key");

            migrationBuilder.RenameColumn(
                name: "LoginProvider",
                table: "AspNetUserLogins",
                newName: "login_provider");

            migrationBuilder.RenameIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                newName: "ix_asp_net_user_logins_user_id");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "AspNetUserClaims",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "AspNetUserClaims",
                newName: "user_id");

            migrationBuilder.RenameColumn(
                name: "ClaimValue",
                table: "AspNetUserClaims",
                newName: "claim_value");

            migrationBuilder.RenameColumn(
                name: "ClaimType",
                table: "AspNetUserClaims",
                newName: "claim_type");

            migrationBuilder.RenameIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                newName: "ix_asp_net_user_claims_user_id");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "AspNetRoles",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "AspNetRoles",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "NormalizedName",
                table: "AspNetRoles",
                newName: "normalized_name");

            migrationBuilder.RenameColumn(
                name: "ConcurrencyStamp",
                table: "AspNetRoles",
                newName: "concurrency_stamp");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "AspNetRoleClaims",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "RoleId",
                table: "AspNetRoleClaims",
                newName: "role_id");

            migrationBuilder.RenameColumn(
                name: "ClaimValue",
                table: "AspNetRoleClaims",
                newName: "claim_value");

            migrationBuilder.RenameColumn(
                name: "ClaimType",
                table: "AspNetRoleClaims",
                newName: "claim_type");

            migrationBuilder.RenameIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                newName: "ix_asp_net_role_claims_role_id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_templates_indicators",
                table: "templates_indicators",
                columns: new[] { "template_id", "indicator_id" });

            migrationBuilder.AddPrimaryKey(
                name: "pk_templates",
                table: "templates",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_shifts",
                table: "shifts",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_shift_schedules",
                table: "shift_schedules",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_products",
                table: "products",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_positions",
                table: "positions",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_operations",
                table: "operations",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_indicators",
                table: "indicators",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_forms",
                table: "forms",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_form_rows",
                table: "form_rows",
                columns: new[] { "form_id", "order" });

            migrationBuilder.AddPrimaryKey(
                name: "pk_form_row_values",
                table: "form_row_values",
                columns: new[] { "form_id", "form_row_order", "indicator_id" });

            migrationBuilder.AddPrimaryKey(
                name: "pk_enterprises",
                table: "enterprises",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_employees",
                table: "employees",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_downtime_reason_groups",
                table: "downtime_reason_groups",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_departments",
                table: "departments",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_auxiliary_operations",
                table: "auxiliary_operations",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_asp_net_user_tokens",
                table: "AspNetUserTokens",
                columns: new[] { "user_id", "login_provider", "name" });

            migrationBuilder.AddPrimaryKey(
                name: "pk_asp_net_users",
                table: "AspNetUsers",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_asp_net_user_roles",
                table: "AspNetUserRoles",
                columns: new[] { "user_id", "role_id" });

            migrationBuilder.AddPrimaryKey(
                name: "pk_asp_net_user_logins",
                table: "AspNetUserLogins",
                columns: new[] { "login_provider", "provider_key" });

            migrationBuilder.AddPrimaryKey(
                name: "pk_asp_net_user_claims",
                table: "AspNetUserClaims",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_asp_net_roles",
                table: "AspNetRoles",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_asp_net_role_claims",
                table: "AspNetRoleClaims",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_asp_net_role_claims_asp_net_roles_role_id",
                table: "AspNetRoleClaims",
                column: "role_id",
                principalTable: "AspNetRoles",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_asp_net_user_claims_asp_net_users_user_id",
                table: "AspNetUserClaims",
                column: "user_id",
                principalTable: "AspNetUsers",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_asp_net_user_logins_asp_net_users_user_id",
                table: "AspNetUserLogins",
                column: "user_id",
                principalTable: "AspNetUsers",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_asp_net_user_roles_asp_net_roles_role_id",
                table: "AspNetUserRoles",
                column: "role_id",
                principalTable: "AspNetRoles",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_asp_net_user_roles_asp_net_users_user_id",
                table: "AspNetUserRoles",
                column: "user_id",
                principalTable: "AspNetUsers",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_asp_net_user_tokens_asp_net_users_user_id",
                table: "AspNetUserTokens",
                column: "user_id",
                principalTable: "AspNetUsers",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_departments_enterprises_enterprise_id",
                table: "departments",
                column: "enterprise_id",
                principalTable: "enterprises",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_employees_departments_department_id",
                table: "employees",
                column: "department_id",
                principalTable: "departments",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_employees_positions_position_id",
                table: "employees",
                column: "position_id",
                principalTable: "positions",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_employees_users_user_id",
                table: "employees",
                column: "user_id",
                principalTable: "AspNetUsers",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_form_row_values_form_rows_form_id_form_row_order",
                table: "form_row_values",
                columns: new[] { "form_id", "form_row_order" },
                principalTable: "form_rows",
                principalColumns: new[] { "form_id", "order" },
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_form_row_values_indicators_indicator_id",
                table: "form_row_values",
                column: "indicator_id",
                principalTable: "indicators",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_form_rows_forms_form_id",
                table: "form_rows",
                column: "form_id",
                principalTable: "forms",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_forms_departments_department_id",
                table: "forms",
                column: "department_id",
                principalTable: "departments",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_forms_employees_executor_id",
                table: "forms",
                column: "executor_id",
                principalTable: "employees",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_forms_shifts_shift_id",
                table: "forms",
                column: "shift_id",
                principalTable: "shifts",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_forms_users_creator_id",
                table: "forms",
                column: "creator_id",
                principalTable: "AspNetUsers",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_forms_users_last_editor_id",
                table: "forms",
                column: "last_editor_id",
                principalTable: "AspNetUsers",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_operations_operations_based_operation_id",
                table: "operations",
                column: "based_operation_id",
                principalTable: "operations",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_operations_products_based_product_id",
                table: "operations",
                column: "based_product_id",
                principalTable: "products",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_products_enterprises_enterprise_id",
                table: "products",
                column: "enterprise_id",
                principalTable: "enterprises",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_shift_schedules_auxiliary_operations_auxiliary_operation_id",
                table: "shift_schedules",
                column: "auxiliary_operation_id",
                principalTable: "auxiliary_operations",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_shift_schedules_shifts_shift_id",
                table: "shift_schedules",
                column: "shift_id",
                principalTable: "shifts",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_templates_indicators_indicators_indicator_id",
                table: "templates_indicators",
                column: "indicator_id",
                principalTable: "indicators",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_templates_indicators_templates_template_dbo_id",
                table: "templates_indicators",
                column: "template_dbo_id",
                principalTable: "templates",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_templates_indicators_templates_template_id",
                table: "templates_indicators",
                column: "template_id",
                principalTable: "templates",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_asp_net_role_claims_asp_net_roles_role_id",
                table: "AspNetRoleClaims");

            migrationBuilder.DropForeignKey(
                name: "fk_asp_net_user_claims_asp_net_users_user_id",
                table: "AspNetUserClaims");

            migrationBuilder.DropForeignKey(
                name: "fk_asp_net_user_logins_asp_net_users_user_id",
                table: "AspNetUserLogins");

            migrationBuilder.DropForeignKey(
                name: "fk_asp_net_user_roles_asp_net_roles_role_id",
                table: "AspNetUserRoles");

            migrationBuilder.DropForeignKey(
                name: "fk_asp_net_user_roles_asp_net_users_user_id",
                table: "AspNetUserRoles");

            migrationBuilder.DropForeignKey(
                name: "fk_asp_net_user_tokens_asp_net_users_user_id",
                table: "AspNetUserTokens");

            migrationBuilder.DropForeignKey(
                name: "fk_departments_enterprises_enterprise_id",
                table: "departments");

            migrationBuilder.DropForeignKey(
                name: "fk_employees_departments_department_id",
                table: "employees");

            migrationBuilder.DropForeignKey(
                name: "fk_employees_positions_position_id",
                table: "employees");

            migrationBuilder.DropForeignKey(
                name: "fk_employees_users_user_id",
                table: "employees");

            migrationBuilder.DropForeignKey(
                name: "fk_form_row_values_form_rows_form_id_form_row_order",
                table: "form_row_values");

            migrationBuilder.DropForeignKey(
                name: "fk_form_row_values_indicators_indicator_id",
                table: "form_row_values");

            migrationBuilder.DropForeignKey(
                name: "fk_form_rows_forms_form_id",
                table: "form_rows");

            migrationBuilder.DropForeignKey(
                name: "fk_forms_departments_department_id",
                table: "forms");

            migrationBuilder.DropForeignKey(
                name: "fk_forms_employees_executor_id",
                table: "forms");

            migrationBuilder.DropForeignKey(
                name: "fk_forms_shifts_shift_id",
                table: "forms");

            migrationBuilder.DropForeignKey(
                name: "fk_forms_users_creator_id",
                table: "forms");

            migrationBuilder.DropForeignKey(
                name: "fk_forms_users_last_editor_id",
                table: "forms");

            migrationBuilder.DropForeignKey(
                name: "fk_operations_operations_based_operation_id",
                table: "operations");

            migrationBuilder.DropForeignKey(
                name: "fk_operations_products_based_product_id",
                table: "operations");

            migrationBuilder.DropForeignKey(
                name: "fk_products_enterprises_enterprise_id",
                table: "products");

            migrationBuilder.DropForeignKey(
                name: "fk_shift_schedules_auxiliary_operations_auxiliary_operation_id",
                table: "shift_schedules");

            migrationBuilder.DropForeignKey(
                name: "fk_shift_schedules_shifts_shift_id",
                table: "shift_schedules");

            migrationBuilder.DropForeignKey(
                name: "fk_templates_indicators_indicators_indicator_id",
                table: "templates_indicators");

            migrationBuilder.DropForeignKey(
                name: "fk_templates_indicators_templates_template_dbo_id",
                table: "templates_indicators");

            migrationBuilder.DropForeignKey(
                name: "fk_templates_indicators_templates_template_id",
                table: "templates_indicators");

            migrationBuilder.DropPrimaryKey(
                name: "pk_templates_indicators",
                table: "templates_indicators");

            migrationBuilder.DropPrimaryKey(
                name: "pk_templates",
                table: "templates");

            migrationBuilder.DropPrimaryKey(
                name: "pk_shifts",
                table: "shifts");

            migrationBuilder.DropPrimaryKey(
                name: "pk_shift_schedules",
                table: "shift_schedules");

            migrationBuilder.DropPrimaryKey(
                name: "pk_products",
                table: "products");

            migrationBuilder.DropPrimaryKey(
                name: "pk_positions",
                table: "positions");

            migrationBuilder.DropPrimaryKey(
                name: "pk_operations",
                table: "operations");

            migrationBuilder.DropPrimaryKey(
                name: "pk_indicators",
                table: "indicators");

            migrationBuilder.DropPrimaryKey(
                name: "pk_forms",
                table: "forms");

            migrationBuilder.DropPrimaryKey(
                name: "pk_form_rows",
                table: "form_rows");

            migrationBuilder.DropPrimaryKey(
                name: "pk_form_row_values",
                table: "form_row_values");

            migrationBuilder.DropPrimaryKey(
                name: "pk_enterprises",
                table: "enterprises");

            migrationBuilder.DropPrimaryKey(
                name: "pk_employees",
                table: "employees");

            migrationBuilder.DropPrimaryKey(
                name: "pk_downtime_reason_groups",
                table: "downtime_reason_groups");

            migrationBuilder.DropPrimaryKey(
                name: "pk_departments",
                table: "departments");

            migrationBuilder.DropPrimaryKey(
                name: "pk_auxiliary_operations",
                table: "auxiliary_operations");

            migrationBuilder.DropPrimaryKey(
                name: "pk_asp_net_user_tokens",
                table: "AspNetUserTokens");

            migrationBuilder.DropPrimaryKey(
                name: "pk_asp_net_users",
                table: "AspNetUsers");

            migrationBuilder.DropPrimaryKey(
                name: "pk_asp_net_user_roles",
                table: "AspNetUserRoles");

            migrationBuilder.DropPrimaryKey(
                name: "pk_asp_net_user_logins",
                table: "AspNetUserLogins");

            migrationBuilder.DropPrimaryKey(
                name: "pk_asp_net_user_claims",
                table: "AspNetUserClaims");

            migrationBuilder.DropPrimaryKey(
                name: "pk_asp_net_roles",
                table: "AspNetRoles");

            migrationBuilder.DropPrimaryKey(
                name: "pk_asp_net_role_claims",
                table: "AspNetRoleClaims");

            migrationBuilder.RenameColumn(
                name: "order",
                table: "templates_indicators",
                newName: "Order");

            migrationBuilder.RenameColumn(
                name: "template_dbo_id",
                table: "templates_indicators",
                newName: "TemplateDboId");

            migrationBuilder.RenameColumn(
                name: "indicator_id",
                table: "templates_indicators",
                newName: "IndicatorId");

            migrationBuilder.RenameColumn(
                name: "template_id",
                table: "templates_indicators",
                newName: "TemplateId");

            migrationBuilder.RenameIndex(
                name: "ix_templates_indicators_template_dbo_id",
                table: "templates_indicators",
                newName: "IX_templates_indicators_TemplateDboId");

            migrationBuilder.RenameIndex(
                name: "ix_templates_indicators_indicator_id",
                table: "templates_indicators",
                newName: "IX_templates_indicators_IndicatorId");

            migrationBuilder.RenameColumn(
                name: "version",
                table: "templates",
                newName: "Version");

            migrationBuilder.RenameColumn(
                name: "name",
                table: "templates",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "templates",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "pa_type_id",
                table: "templates",
                newName: "PaTypeId");

            migrationBuilder.RenameColumn(
                name: "name",
                table: "shifts",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "shifts",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "start_time",
                table: "shifts",
                newName: "StartTime");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "shift_schedules",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "start_time",
                table: "shift_schedules",
                newName: "StartTime");

            migrationBuilder.RenameColumn(
                name: "shift_id",
                table: "shift_schedules",
                newName: "ShiftId");

            migrationBuilder.RenameColumn(
                name: "auxiliary_operation_id",
                table: "shift_schedules",
                newName: "AuxiliaryOperationId");

            migrationBuilder.RenameIndex(
                name: "ix_shift_schedules_shift_id",
                table: "shift_schedules",
                newName: "IX_shift_schedules_ShiftId");

            migrationBuilder.RenameIndex(
                name: "ix_shift_schedules_auxiliary_operation_id",
                table: "shift_schedules",
                newName: "IX_shift_schedules_AuxiliaryOperationId");

            migrationBuilder.RenameColumn(
                name: "name",
                table: "products",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "products",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "tact_time_in_seconds",
                table: "products",
                newName: "TactTimeInSeconds");

            migrationBuilder.RenameColumn(
                name: "enterprise_id",
                table: "products",
                newName: "EnterpriseId");

            migrationBuilder.RenameIndex(
                name: "ix_products_enterprise_id",
                table: "products",
                newName: "IX_products_EnterpriseId");

            migrationBuilder.RenameColumn(
                name: "role",
                table: "positions",
                newName: "Role");

            migrationBuilder.RenameColumn(
                name: "name",
                table: "positions",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "positions",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "name",
                table: "operations",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "operations",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "duration_in_seconds",
                table: "operations",
                newName: "DurationInSeconds");

            migrationBuilder.RenameColumn(
                name: "based_product_id",
                table: "operations",
                newName: "BasedProductId");

            migrationBuilder.RenameColumn(
                name: "based_operation_id",
                table: "operations",
                newName: "BasedOperationId");

            migrationBuilder.RenameColumn(
                name: "based_on_type",
                table: "operations",
                newName: "BasedOnType");

            migrationBuilder.RenameIndex(
                name: "ix_operations_based_product_id",
                table: "operations",
                newName: "IX_operations_BasedProductId");

            migrationBuilder.RenameIndex(
                name: "ix_operations_based_operation_id",
                table: "operations",
                newName: "IX_operations_BasedOperationId");

            migrationBuilder.RenameColumn(
                name: "name",
                table: "indicators",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "formula",
                table: "indicators",
                newName: "Formula");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "indicators",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "value_type",
                table: "indicators",
                newName: "ValueType");

            migrationBuilder.RenameColumn(
                name: "value_selector",
                table: "indicators",
                newName: "ValueSelector");

            migrationBuilder.RenameColumn(
                name: "input_type",
                table: "indicators",
                newName: "InputType");

            migrationBuilder.RenameColumn(
                name: "has_summation",
                table: "indicators",
                newName: "HasSummation");

            migrationBuilder.RenameColumn(
                name: "status",
                table: "forms",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "context",
                table: "forms",
                newName: "Context");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "forms",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "update_date",
                table: "forms",
                newName: "UpdateDate");

            migrationBuilder.RenameColumn(
                name: "total_values",
                table: "forms",
                newName: "TotalValues");

            migrationBuilder.RenameColumn(
                name: "template_snapshot",
                table: "forms",
                newName: "TemplateSnapshot");

            migrationBuilder.RenameColumn(
                name: "shift_id",
                table: "forms",
                newName: "ShiftId");

            migrationBuilder.RenameColumn(
                name: "pa_type_id",
                table: "forms",
                newName: "PaTypeId");

            migrationBuilder.RenameColumn(
                name: "last_editor_id",
                table: "forms",
                newName: "LastEditorId");

            migrationBuilder.RenameColumn(
                name: "form_date",
                table: "forms",
                newName: "FormDate");

            migrationBuilder.RenameColumn(
                name: "executor_id",
                table: "forms",
                newName: "ExecutorId");

            migrationBuilder.RenameColumn(
                name: "department_id",
                table: "forms",
                newName: "DepartmentId");

            migrationBuilder.RenameColumn(
                name: "creator_id",
                table: "forms",
                newName: "CreatorId");

            migrationBuilder.RenameColumn(
                name: "creation_date",
                table: "forms",
                newName: "CreationDate");

            migrationBuilder.RenameIndex(
                name: "ix_forms_shift_id",
                table: "forms",
                newName: "IX_forms_ShiftId");

            migrationBuilder.RenameIndex(
                name: "ix_forms_last_editor_id",
                table: "forms",
                newName: "IX_forms_LastEditorId");

            migrationBuilder.RenameIndex(
                name: "ix_forms_executor_id",
                table: "forms",
                newName: "IX_forms_ExecutorId");

            migrationBuilder.RenameIndex(
                name: "ix_forms_department_id",
                table: "forms",
                newName: "IX_forms_DepartmentId");

            migrationBuilder.RenameIndex(
                name: "ix_forms_creator_id",
                table: "forms",
                newName: "IX_forms_CreatorId");

            migrationBuilder.RenameColumn(
                name: "order",
                table: "form_rows",
                newName: "Order");

            migrationBuilder.RenameColumn(
                name: "product_id",
                table: "form_rows",
                newName: "ProductId");

            migrationBuilder.RenameColumn(
                name: "is_auxiliary_operation",
                table: "form_rows",
                newName: "IsAuxiliaryOperation");

            migrationBuilder.RenameColumn(
                name: "group_key",
                table: "form_rows",
                newName: "GroupKey");

            migrationBuilder.RenameColumn(
                name: "auxiliary_operation_id",
                table: "form_rows",
                newName: "AuxiliaryOperationId");

            migrationBuilder.RenameColumn(
                name: "form_id",
                table: "form_rows",
                newName: "FormId");

            migrationBuilder.RenameIndex(
                name: "ix_form_rows_form_id",
                table: "form_rows",
                newName: "IX_form_rows_FormId");

            migrationBuilder.RenameColumn(
                name: "value",
                table: "form_row_values",
                newName: "Value");

            migrationBuilder.RenameColumn(
                name: "indicator_id",
                table: "form_row_values",
                newName: "IndicatorId");

            migrationBuilder.RenameColumn(
                name: "form_row_order",
                table: "form_row_values",
                newName: "FormRowOrder");

            migrationBuilder.RenameColumn(
                name: "form_id",
                table: "form_row_values",
                newName: "FormId");

            migrationBuilder.RenameIndex(
                name: "ix_form_row_values_indicator_id",
                table: "form_row_values",
                newName: "IX_form_row_values_IndicatorId");

            migrationBuilder.RenameIndex(
                name: "ix_form_row_values_form_id_form_row_order",
                table: "form_row_values",
                newName: "IX_form_row_values_FormId_FormRowOrder");

            migrationBuilder.RenameColumn(
                name: "name",
                table: "enterprises",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "enterprises",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "email",
                table: "employees",
                newName: "Email");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "employees",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "user_id",
                table: "employees",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "position_id",
                table: "employees",
                newName: "PositionId");

            migrationBuilder.RenameColumn(
                name: "middle_name",
                table: "employees",
                newName: "MiddleName");

            migrationBuilder.RenameColumn(
                name: "last_name",
                table: "employees",
                newName: "LastName");

            migrationBuilder.RenameColumn(
                name: "first_name",
                table: "employees",
                newName: "FirstName");

            migrationBuilder.RenameColumn(
                name: "department_id",
                table: "employees",
                newName: "DepartmentId");

            migrationBuilder.RenameIndex(
                name: "ix_employees_user_id",
                table: "employees",
                newName: "IX_employees_UserId");

            migrationBuilder.RenameIndex(
                name: "ix_employees_position_id",
                table: "employees",
                newName: "IX_employees_PositionId");

            migrationBuilder.RenameIndex(
                name: "ix_employees_department_id",
                table: "employees",
                newName: "IX_employees_DepartmentId");

            migrationBuilder.RenameColumn(
                name: "name",
                table: "downtime_reason_groups",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "description",
                table: "downtime_reason_groups",
                newName: "Description");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "downtime_reason_groups",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "name",
                table: "departments",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "departments",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "enterprise_id",
                table: "departments",
                newName: "EnterpriseId");

            migrationBuilder.RenameIndex(
                name: "ix_departments_enterprise_id",
                table: "departments",
                newName: "IX_departments_EnterpriseId");

            migrationBuilder.RenameColumn(
                name: "name",
                table: "auxiliary_operations",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "auxiliary_operations",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "duration_in_seconds",
                table: "auxiliary_operations",
                newName: "DurationInSeconds");

            migrationBuilder.RenameColumn(
                name: "value",
                table: "AspNetUserTokens",
                newName: "Value");

            migrationBuilder.RenameColumn(
                name: "name",
                table: "AspNetUserTokens",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "login_provider",
                table: "AspNetUserTokens",
                newName: "LoginProvider");

            migrationBuilder.RenameColumn(
                name: "user_id",
                table: "AspNetUserTokens",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "email",
                table: "AspNetUsers",
                newName: "Email");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "AspNetUsers",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "user_name",
                table: "AspNetUsers",
                newName: "UserName");

            migrationBuilder.RenameColumn(
                name: "two_factor_enabled",
                table: "AspNetUsers",
                newName: "TwoFactorEnabled");

            migrationBuilder.RenameColumn(
                name: "security_stamp",
                table: "AspNetUsers",
                newName: "SecurityStamp");

            migrationBuilder.RenameColumn(
                name: "phone_number_confirmed",
                table: "AspNetUsers",
                newName: "PhoneNumberConfirmed");

            migrationBuilder.RenameColumn(
                name: "phone_number",
                table: "AspNetUsers",
                newName: "PhoneNumber");

            migrationBuilder.RenameColumn(
                name: "password_hash",
                table: "AspNetUsers",
                newName: "PasswordHash");

            migrationBuilder.RenameColumn(
                name: "normalized_user_name",
                table: "AspNetUsers",
                newName: "NormalizedUserName");

            migrationBuilder.RenameColumn(
                name: "normalized_email",
                table: "AspNetUsers",
                newName: "NormalizedEmail");

            migrationBuilder.RenameColumn(
                name: "middle_name",
                table: "AspNetUsers",
                newName: "MiddleName");

            migrationBuilder.RenameColumn(
                name: "lockout_end",
                table: "AspNetUsers",
                newName: "LockoutEnd");

            migrationBuilder.RenameColumn(
                name: "lockout_enabled",
                table: "AspNetUsers",
                newName: "LockoutEnabled");

            migrationBuilder.RenameColumn(
                name: "last_name",
                table: "AspNetUsers",
                newName: "LastName");

            migrationBuilder.RenameColumn(
                name: "first_name",
                table: "AspNetUsers",
                newName: "FirstName");

            migrationBuilder.RenameColumn(
                name: "email_confirmed",
                table: "AspNetUsers",
                newName: "EmailConfirmed");

            migrationBuilder.RenameColumn(
                name: "concurrency_stamp",
                table: "AspNetUsers",
                newName: "ConcurrencyStamp");

            migrationBuilder.RenameColumn(
                name: "access_failed_count",
                table: "AspNetUsers",
                newName: "AccessFailedCount");

            migrationBuilder.RenameColumn(
                name: "role_id",
                table: "AspNetUserRoles",
                newName: "RoleId");

            migrationBuilder.RenameColumn(
                name: "user_id",
                table: "AspNetUserRoles",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "ix_asp_net_user_roles_role_id",
                table: "AspNetUserRoles",
                newName: "IX_AspNetUserRoles_RoleId");

            migrationBuilder.RenameColumn(
                name: "user_id",
                table: "AspNetUserLogins",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "provider_display_name",
                table: "AspNetUserLogins",
                newName: "ProviderDisplayName");

            migrationBuilder.RenameColumn(
                name: "provider_key",
                table: "AspNetUserLogins",
                newName: "ProviderKey");

            migrationBuilder.RenameColumn(
                name: "login_provider",
                table: "AspNetUserLogins",
                newName: "LoginProvider");

            migrationBuilder.RenameIndex(
                name: "ix_asp_net_user_logins_user_id",
                table: "AspNetUserLogins",
                newName: "IX_AspNetUserLogins_UserId");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "AspNetUserClaims",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "user_id",
                table: "AspNetUserClaims",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "claim_value",
                table: "AspNetUserClaims",
                newName: "ClaimValue");

            migrationBuilder.RenameColumn(
                name: "claim_type",
                table: "AspNetUserClaims",
                newName: "ClaimType");

            migrationBuilder.RenameIndex(
                name: "ix_asp_net_user_claims_user_id",
                table: "AspNetUserClaims",
                newName: "IX_AspNetUserClaims_UserId");

            migrationBuilder.RenameColumn(
                name: "name",
                table: "AspNetRoles",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "AspNetRoles",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "normalized_name",
                table: "AspNetRoles",
                newName: "NormalizedName");

            migrationBuilder.RenameColumn(
                name: "concurrency_stamp",
                table: "AspNetRoles",
                newName: "ConcurrencyStamp");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "AspNetRoleClaims",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "role_id",
                table: "AspNetRoleClaims",
                newName: "RoleId");

            migrationBuilder.RenameColumn(
                name: "claim_value",
                table: "AspNetRoleClaims",
                newName: "ClaimValue");

            migrationBuilder.RenameColumn(
                name: "claim_type",
                table: "AspNetRoleClaims",
                newName: "ClaimType");

            migrationBuilder.RenameIndex(
                name: "ix_asp_net_role_claims_role_id",
                table: "AspNetRoleClaims",
                newName: "IX_AspNetRoleClaims_RoleId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_templates_indicators",
                table: "templates_indicators",
                columns: new[] { "TemplateId", "IndicatorId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_templates",
                table: "templates",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_shifts",
                table: "shifts",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_shift_schedules",
                table: "shift_schedules",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_products",
                table: "products",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_positions",
                table: "positions",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_operations",
                table: "operations",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_indicators",
                table: "indicators",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_forms",
                table: "forms",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_form_rows",
                table: "form_rows",
                columns: new[] { "FormId", "Order" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_form_row_values",
                table: "form_row_values",
                columns: new[] { "FormId", "FormRowOrder", "IndicatorId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_enterprises",
                table: "enterprises",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_employees",
                table: "employees",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_downtime_reason_groups",
                table: "downtime_reason_groups",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_departments",
                table: "departments",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_auxiliary_operations",
                table: "auxiliary_operations",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AspNetUserTokens",
                table: "AspNetUserTokens",
                columns: new[] { "UserId", "LoginProvider", "Name" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_AspNetUsers",
                table: "AspNetUsers",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AspNetUserRoles",
                table: "AspNetUserRoles",
                columns: new[] { "UserId", "RoleId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_AspNetUserLogins",
                table: "AspNetUserLogins",
                columns: new[] { "LoginProvider", "ProviderKey" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_AspNetUserClaims",
                table: "AspNetUserClaims",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AspNetRoles",
                table: "AspNetRoles",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AspNetRoleClaims",
                table: "AspNetRoleClaims",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId",
                principalTable: "AspNetRoles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                table: "AspNetUserClaims",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                table: "AspNetUserLogins",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId",
                principalTable: "AspNetRoles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                table: "AspNetUserRoles",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                table: "AspNetUserTokens",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_departments_enterprises_EnterpriseId",
                table: "departments",
                column: "EnterpriseId",
                principalTable: "enterprises",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_employees_AspNetUsers_UserId",
                table: "employees",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_employees_departments_DepartmentId",
                table: "employees",
                column: "DepartmentId",
                principalTable: "departments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_employees_positions_PositionId",
                table: "employees",
                column: "PositionId",
                principalTable: "positions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_form_row_values_form_rows_FormId_FormRowOrder",
                table: "form_row_values",
                columns: new[] { "FormId", "FormRowOrder" },
                principalTable: "form_rows",
                principalColumns: new[] { "FormId", "Order" },
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_form_row_values_indicators_IndicatorId",
                table: "form_row_values",
                column: "IndicatorId",
                principalTable: "indicators",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_form_rows_forms_FormId",
                table: "form_rows",
                column: "FormId",
                principalTable: "forms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_forms_AspNetUsers_CreatorId",
                table: "forms",
                column: "CreatorId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_forms_AspNetUsers_LastEditorId",
                table: "forms",
                column: "LastEditorId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_forms_departments_DepartmentId",
                table: "forms",
                column: "DepartmentId",
                principalTable: "departments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_forms_employees_ExecutorId",
                table: "forms",
                column: "ExecutorId",
                principalTable: "employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_forms_shifts_ShiftId",
                table: "forms",
                column: "ShiftId",
                principalTable: "shifts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_operations_operations_BasedOperationId",
                table: "operations",
                column: "BasedOperationId",
                principalTable: "operations",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_operations_products_BasedProductId",
                table: "operations",
                column: "BasedProductId",
                principalTable: "products",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_products_enterprises_EnterpriseId",
                table: "products",
                column: "EnterpriseId",
                principalTable: "enterprises",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_shift_schedules_auxiliary_operations_AuxiliaryOperationId",
                table: "shift_schedules",
                column: "AuxiliaryOperationId",
                principalTable: "auxiliary_operations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_shift_schedules_shifts_ShiftId",
                table: "shift_schedules",
                column: "ShiftId",
                principalTable: "shifts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_templates_indicators_indicators_IndicatorId",
                table: "templates_indicators",
                column: "IndicatorId",
                principalTable: "indicators",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_templates_indicators_templates_TemplateDboId",
                table: "templates_indicators",
                column: "TemplateDboId",
                principalTable: "templates",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_templates_indicators_templates_TemplateId",
                table: "templates_indicators",
                column: "TemplateId",
                principalTable: "templates",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
