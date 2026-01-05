using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ProductionAnalysis.Data.Migrations
{
    /// <inheritdoc />
    public partial class renameAdditionalToAuxiliary : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_shift_schedules_additional_operations_AdditionalOperationId",
                table: "shift_schedules");

            migrationBuilder.DropTable(
                name: "additional_operations");

            migrationBuilder.RenameColumn(
                name: "AdditionalOperationId",
                table: "shift_schedules",
                newName: "AuxiliaryOperationId");

            migrationBuilder.RenameIndex(
                name: "IX_shift_schedules_AdditionalOperationId",
                table: "shift_schedules",
                newName: "IX_shift_schedules_AuxiliaryOperationId");

            migrationBuilder.RenameColumn(
                name: "IsAdditionalOperation",
                table: "form_rows",
                newName: "IsAuxiliaryOperation");

            migrationBuilder.RenameColumn(
                name: "AdditionalOperationId",
                table: "form_rows",
                newName: "AuxiliaryOperationId");

            migrationBuilder.CreateTable(
                name: "auxiliary_operations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    DurationInSeconds = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_auxiliary_operations", x => x.Id);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_shift_schedules_auxiliary_operations_AuxiliaryOperationId",
                table: "shift_schedules",
                column: "AuxiliaryOperationId",
                principalTable: "auxiliary_operations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_shift_schedules_auxiliary_operations_AuxiliaryOperationId",
                table: "shift_schedules");

            migrationBuilder.DropTable(
                name: "auxiliary_operations");

            migrationBuilder.RenameColumn(
                name: "AuxiliaryOperationId",
                table: "shift_schedules",
                newName: "AdditionalOperationId");

            migrationBuilder.RenameIndex(
                name: "IX_shift_schedules_AuxiliaryOperationId",
                table: "shift_schedules",
                newName: "IX_shift_schedules_AdditionalOperationId");

            migrationBuilder.RenameColumn(
                name: "IsAuxiliaryOperation",
                table: "form_rows",
                newName: "IsAdditionalOperation");

            migrationBuilder.RenameColumn(
                name: "AuxiliaryOperationId",
                table: "form_rows",
                newName: "AdditionalOperationId");

            migrationBuilder.CreateTable(
                name: "additional_operations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DurationInSeconds = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_additional_operations", x => x.Id);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_shift_schedules_additional_operations_AdditionalOperationId",
                table: "shift_schedules",
                column: "AdditionalOperationId",
                principalTable: "additional_operations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
