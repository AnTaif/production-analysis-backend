using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProductionAnalysis.Data.Migrations
{
    /// <inheritdoc />
    public partial class addShiftAndDepartmentLink : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DepartmentId",
                table: "forms",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ShiftId",
                table: "forms",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "employees",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_forms_DepartmentId",
                table: "forms",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_forms_ShiftId",
                table: "forms",
                column: "ShiftId");

            migrationBuilder.CreateIndex(
                name: "IX_employees_UserId",
                table: "employees",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_employees_AspNetUsers_UserId",
                table: "employees",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_forms_departments_DepartmentId",
                table: "forms",
                column: "DepartmentId",
                principalTable: "departments",
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_employees_AspNetUsers_UserId",
                table: "employees");

            migrationBuilder.DropForeignKey(
                name: "FK_forms_departments_DepartmentId",
                table: "forms");

            migrationBuilder.DropForeignKey(
                name: "FK_forms_shifts_ShiftId",
                table: "forms");

            migrationBuilder.DropIndex(
                name: "IX_forms_DepartmentId",
                table: "forms");

            migrationBuilder.DropIndex(
                name: "IX_forms_ShiftId",
                table: "forms");

            migrationBuilder.DropIndex(
                name: "IX_employees_UserId",
                table: "employees");

            migrationBuilder.DropColumn(
                name: "DepartmentId",
                table: "forms");

            migrationBuilder.DropColumn(
                name: "ShiftId",
                table: "forms");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "employees");
        }
    }
}
