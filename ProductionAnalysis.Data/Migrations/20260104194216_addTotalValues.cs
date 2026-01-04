using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProductionAnalysis.Data.Migrations
{
    /// <inheritdoc />
    public partial class addTotalValues : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TotalValues",
                table: "forms",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CumulativeValue",
                table: "form_row_values",
                type: "jsonb",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TotalValues",
                table: "forms");

            migrationBuilder.DropColumn(
                name: "CumulativeValue",
                table: "form_row_values");
        }
    }
}
