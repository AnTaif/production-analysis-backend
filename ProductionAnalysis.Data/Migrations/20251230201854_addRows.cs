using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ProductionAnalysis.Data.Migrations
{
    /// <inheritdoc />
    public partial class addRows : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "form_rows",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FormId = table.Column<int>(type: "integer", nullable: false),
                    Order = table.Column<short>(type: "smallint", nullable: false),
                    IsAdditionalOperation = table.Column<bool>(type: "boolean", nullable: false),
                    AdditionalOperationId = table.Column<int>(type: "integer", nullable: true),
                    FormDboId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_form_rows", x => x.Id);
                    table.ForeignKey(
                        name: "FK_form_rows_forms_FormDboId",
                        column: x => x.FormDboId,
                        principalTable: "forms",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_form_rows_forms_FormId",
                        column: x => x.FormId,
                        principalTable: "forms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "shift_schedules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ShiftId = table.Column<int>(type: "integer", nullable: false),
                    AdditionalOperationId = table.Column<int>(type: "integer", nullable: false),
                    StartTime = table.Column<TimeOnly>(type: "time without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_shift_schedules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_shift_schedules_additional_operations_AdditionalOperationId",
                        column: x => x.AdditionalOperationId,
                        principalTable: "additional_operations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_shift_schedules_shifts_ShiftId",
                        column: x => x.ShiftId,
                        principalTable: "shifts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "form_row_values",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FormRowId = table.Column<int>(type: "integer", nullable: false),
                    IndicatorId = table.Column<int>(type: "integer", nullable: false),
                    Value = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_form_row_values", x => x.Id);
                    table.ForeignKey(
                        name: "FK_form_row_values_form_rows_FormRowId",
                        column: x => x.FormRowId,
                        principalTable: "form_rows",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_form_row_values_indicators_IndicatorId",
                        column: x => x.IndicatorId,
                        principalTable: "indicators",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_form_row_values_FormRowId",
                table: "form_row_values",
                column: "FormRowId");

            migrationBuilder.CreateIndex(
                name: "IX_form_row_values_FormRowId_IndicatorId",
                table: "form_row_values",
                columns: new[] { "FormRowId", "IndicatorId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_form_row_values_IndicatorId",
                table: "form_row_values",
                column: "IndicatorId");

            migrationBuilder.CreateIndex(
                name: "IX_form_rows_FormDboId",
                table: "form_rows",
                column: "FormDboId");

            migrationBuilder.CreateIndex(
                name: "IX_form_rows_FormId",
                table: "form_rows",
                column: "FormId");

            migrationBuilder.CreateIndex(
                name: "IX_form_rows_FormId_Order",
                table: "form_rows",
                columns: new[] { "FormId", "Order" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_shift_schedules_AdditionalOperationId",
                table: "shift_schedules",
                column: "AdditionalOperationId");

            migrationBuilder.CreateIndex(
                name: "IX_shift_schedules_ShiftId",
                table: "shift_schedules",
                column: "ShiftId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "form_row_values");

            migrationBuilder.DropTable(
                name: "shift_schedules");

            migrationBuilder.DropTable(
                name: "form_rows");
        }
    }
}
