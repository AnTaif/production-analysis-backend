using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProductionAnalysis.Data.Migrations
{
    /// <inheritdoc />
    public partial class updateRows : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_form_rows_forms_FormDboId",
                table: "form_rows");

            migrationBuilder.DropIndex(
                name: "IX_form_rows_FormDboId",
                table: "form_rows");

            migrationBuilder.DropColumn(
                name: "FormDboId",
                table: "form_rows");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FormDboId",
                table: "form_rows",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_form_rows_FormDboId",
                table: "form_rows",
                column: "FormDboId");

            migrationBuilder.AddForeignKey(
                name: "FK_form_rows_forms_FormDboId",
                table: "form_rows",
                column: "FormDboId",
                principalTable: "forms",
                principalColumn: "Id");
        }
    }
}
