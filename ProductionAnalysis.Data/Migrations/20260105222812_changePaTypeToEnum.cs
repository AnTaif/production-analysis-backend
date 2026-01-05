using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ProductionAnalysis.Data.Migrations
{
    /// <inheritdoc />
    public partial class changePaTypeToEnum : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_forms_pa_types_PaTypeId",
                table: "forms");

            migrationBuilder.DropForeignKey(
                name: "FK_templates_pa_types_PaTypeId",
                table: "templates");

            migrationBuilder.DropTable(
                name: "pa_types");

            migrationBuilder.DropIndex(
                name: "IX_templates_PaTypeId",
                table: "templates");

            migrationBuilder.DropIndex(
                name: "IX_forms_PaTypeId",
                table: "forms");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "pa_types",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pa_types", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_templates_PaTypeId",
                table: "templates",
                column: "PaTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_forms_PaTypeId",
                table: "forms",
                column: "PaTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_forms_pa_types_PaTypeId",
                table: "forms",
                column: "PaTypeId",
                principalTable: "pa_types",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_templates_pa_types_PaTypeId",
                table: "templates",
                column: "PaTypeId",
                principalTable: "pa_types",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
