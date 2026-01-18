using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProductionAnalysis.Data.Migrations
{
    /// <inheritdoc />
    public partial class renameColumnInForms : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_forms_employees_executor_id",
                table: "forms");

            migrationBuilder.RenameColumn(
                name: "executor_id",
                table: "forms",
                newName: "assignee_id");

            migrationBuilder.RenameIndex(
                name: "ix_forms_executor_id",
                table: "forms",
                newName: "ix_forms_assignee_id");

            migrationBuilder.AddForeignKey(
                name: "fk_forms_employees_assignee_id",
                table: "forms",
                column: "assignee_id",
                principalTable: "employees",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_forms_employees_assignee_id",
                table: "forms");

            migrationBuilder.RenameColumn(
                name: "assignee_id",
                table: "forms",
                newName: "executor_id");

            migrationBuilder.RenameIndex(
                name: "ix_forms_assignee_id",
                table: "forms",
                newName: "ix_forms_executor_id");

            migrationBuilder.AddForeignKey(
                name: "fk_forms_employees_executor_id",
                table: "forms",
                column: "executor_id",
                principalTable: "employees",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
