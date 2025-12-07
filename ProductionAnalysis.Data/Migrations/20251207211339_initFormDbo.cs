using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProductionAnalysis.Data.Migrations
{
    /// <inheritdoc />
    public partial class initFormDbo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "forms",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PaTypeId = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Context = table.Column<string>(type: "jsonb", nullable: false),
                    TemplateSnapshot = table.Column<string>(type: "jsonb", nullable: false),
                    CreationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: false),
                    LastEditorId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_forms", x => x.Id);
                    table.ForeignKey(
                        name: "FK_forms_AspNetUsers_CreatorId",
                        column: x => x.CreatorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_forms_AspNetUsers_LastEditorId",
                        column: x => x.LastEditorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_forms_pa_types_PaTypeId",
                        column: x => x.PaTypeId,
                        principalTable: "pa_types",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_forms_CreatorId",
                table: "forms",
                column: "CreatorId");

            migrationBuilder.CreateIndex(
                name: "IX_forms_LastEditorId",
                table: "forms",
                column: "LastEditorId");

            migrationBuilder.CreateIndex(
                name: "IX_forms_PaTypeId",
                table: "forms",
                column: "PaTypeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "forms");
        }
    }
}
