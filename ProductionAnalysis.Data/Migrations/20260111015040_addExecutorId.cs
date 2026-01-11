using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProductionAnalysis.Data.Migrations
{
    /// <inheritdoc />
    public partial class addExecutorId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ExecutorId",
                table: "forms",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "GroupKey",
                table: "form_rows",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_forms_ExecutorId",
                table: "forms",
                column: "ExecutorId");

            migrationBuilder.AddForeignKey(
                name: "FK_forms_employees_ExecutorId",
                table: "forms",
                column: "ExecutorId",
                principalTable: "employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_forms_employees_ExecutorId",
                table: "forms");

            migrationBuilder.DropIndex(
                name: "IX_forms_ExecutorId",
                table: "forms");

            migrationBuilder.DropColumn(
                name: "ExecutorId",
                table: "forms");

            migrationBuilder.DropColumn(
                name: "GroupKey",
                table: "form_rows");
        }
    }
}
