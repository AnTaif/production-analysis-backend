using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ProductionAnalysis.Data.Migrations
{
    /// <inheritdoc />
    public partial class addPositions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Position",
                table: "employees");

            migrationBuilder.AddColumn<int>(
                name: "PositionId",
                table: "employees",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "positions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Role = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_positions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_employees_PositionId",
                table: "employees",
                column: "PositionId");

            migrationBuilder.AddForeignKey(
                name: "FK_employees_positions_PositionId",
                table: "employees",
                column: "PositionId",
                principalTable: "positions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_employees_positions_PositionId",
                table: "employees");

            migrationBuilder.DropTable(
                name: "positions");

            migrationBuilder.DropIndex(
                name: "IX_employees_PositionId",
                table: "employees");

            migrationBuilder.DropColumn(
                name: "PositionId",
                table: "employees");

            migrationBuilder.AddColumn<string>(
                name: "Position",
                table: "employees",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");
        }
    }
}
