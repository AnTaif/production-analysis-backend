using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ProductionAnalysis.Data.Migrations
{
    /// <inheritdoc />
    public partial class removeIdFromRows : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_form_row_values_form_rows_FormRowId",
                table: "form_row_values");

            migrationBuilder.DropPrimaryKey(
                name: "PK_form_rows",
                table: "form_rows");

            migrationBuilder.DropIndex(
                name: "IX_form_rows_FormId_Order",
                table: "form_rows");

            migrationBuilder.DropPrimaryKey(
                name: "PK_form_row_values",
                table: "form_row_values");

            migrationBuilder.DropIndex(
                name: "IX_form_row_values_FormRowId",
                table: "form_row_values");

            migrationBuilder.DropIndex(
                name: "IX_form_row_values_FormRowId_IndicatorId",
                table: "form_row_values");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "form_rows");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "form_row_values");

            migrationBuilder.RenameColumn(
                name: "FormRowId",
                table: "form_row_values",
                newName: "FormId");

            migrationBuilder.AddColumn<short>(
                name: "FormRowOrder",
                table: "form_row_values",
                type: "smallint",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_form_rows",
                table: "form_rows",
                columns: new[] { "FormId", "Order" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_form_row_values",
                table: "form_row_values",
                columns: new[] { "FormId", "FormRowOrder", "IndicatorId" });

            migrationBuilder.CreateIndex(
                name: "IX_form_row_values_FormId_FormRowOrder",
                table: "form_row_values",
                columns: new[] { "FormId", "FormRowOrder" });

            migrationBuilder.AddForeignKey(
                name: "FK_form_row_values_form_rows_FormId_FormRowOrder",
                table: "form_row_values",
                columns: new[] { "FormId", "FormRowOrder" },
                principalTable: "form_rows",
                principalColumns: new[] { "FormId", "Order" },
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_form_row_values_form_rows_FormId_FormRowOrder",
                table: "form_row_values");

            migrationBuilder.DropPrimaryKey(
                name: "PK_form_rows",
                table: "form_rows");

            migrationBuilder.DropPrimaryKey(
                name: "PK_form_row_values",
                table: "form_row_values");

            migrationBuilder.DropIndex(
                name: "IX_form_row_values_FormId_FormRowOrder",
                table: "form_row_values");

            migrationBuilder.DropColumn(
                name: "FormRowOrder",
                table: "form_row_values");

            migrationBuilder.RenameColumn(
                name: "FormId",
                table: "form_row_values",
                newName: "FormRowId");

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "form_rows",
                type: "integer",
                nullable: false,
                defaultValue: 0)
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "form_row_values",
                type: "integer",
                nullable: false,
                defaultValue: 0)
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddPrimaryKey(
                name: "PK_form_rows",
                table: "form_rows",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_form_row_values",
                table: "form_row_values",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_form_rows_FormId_Order",
                table: "form_rows",
                columns: new[] { "FormId", "Order" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_form_row_values_FormRowId",
                table: "form_row_values",
                column: "FormRowId");

            migrationBuilder.CreateIndex(
                name: "IX_form_row_values_FormRowId_IndicatorId",
                table: "form_row_values",
                columns: new[] { "FormRowId", "IndicatorId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_form_row_values_form_rows_FormRowId",
                table: "form_row_values",
                column: "FormRowId",
                principalTable: "form_rows",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
