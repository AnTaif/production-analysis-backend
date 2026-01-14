using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProductionAnalysis.Data.Migrations
{
    /// <inheritdoc />
    public partial class changeOnDeleteBehaviors : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
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

            migrationBuilder.AddForeignKey(
                name: "FK_forms_AspNetUsers_CreatorId",
                table: "forms",
                column: "CreatorId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_forms_AspNetUsers_LastEditorId",
                table: "forms",
                column: "LastEditorId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_forms_departments_DepartmentId",
                table: "forms",
                column: "DepartmentId",
                principalTable: "departments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_forms_employees_ExecutorId",
                table: "forms",
                column: "ExecutorId",
                principalTable: "employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_forms_shifts_ShiftId",
                table: "forms",
                column: "ShiftId",
                principalTable: "shifts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
