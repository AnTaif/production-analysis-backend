using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProductionAnalysis.Data.Migrations
{
    /// <inheritdoc />
    public partial class addindicatorOrders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_templates_indicators_indicators_IndicatorsId",
                table: "templates_indicators");

            migrationBuilder.DropForeignKey(
                name: "FK_templates_indicators_templates_TemplatesId",
                table: "templates_indicators");

            migrationBuilder.DropPrimaryKey(
                name: "PK_templates_indicators",
                table: "templates_indicators");

            migrationBuilder.DropIndex(
                name: "IX_templates_indicators_TemplatesId",
                table: "templates_indicators");

            migrationBuilder.RenameColumn(
                name: "TemplatesId",
                table: "templates_indicators",
                newName: "Order");

            migrationBuilder.RenameColumn(
                name: "IndicatorsId",
                table: "templates_indicators",
                newName: "IndicatorId");

            migrationBuilder.AddColumn<int>(
                name: "TemplateId",
                table: "templates_indicators",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TemplateDboId",
                table: "templates_indicators",
                type: "integer",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_templates_indicators",
                table: "templates_indicators",
                columns: new[] { "TemplateId", "IndicatorId" });

            migrationBuilder.CreateIndex(
                name: "IX_templates_indicators_IndicatorId",
                table: "templates_indicators",
                column: "IndicatorId");

            migrationBuilder.CreateIndex(
                name: "IX_templates_indicators_TemplateDboId",
                table: "templates_indicators",
                column: "TemplateDboId");

            migrationBuilder.AddForeignKey(
                name: "FK_templates_indicators_indicators_IndicatorId",
                table: "templates_indicators",
                column: "IndicatorId",
                principalTable: "indicators",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_templates_indicators_templates_TemplateDboId",
                table: "templates_indicators",
                column: "TemplateDboId",
                principalTable: "templates",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_templates_indicators_templates_TemplateId",
                table: "templates_indicators",
                column: "TemplateId",
                principalTable: "templates",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_templates_indicators_indicators_IndicatorId",
                table: "templates_indicators");

            migrationBuilder.DropForeignKey(
                name: "FK_templates_indicators_templates_TemplateDboId",
                table: "templates_indicators");

            migrationBuilder.DropForeignKey(
                name: "FK_templates_indicators_templates_TemplateId",
                table: "templates_indicators");

            migrationBuilder.DropPrimaryKey(
                name: "PK_templates_indicators",
                table: "templates_indicators");

            migrationBuilder.DropIndex(
                name: "IX_templates_indicators_IndicatorId",
                table: "templates_indicators");

            migrationBuilder.DropIndex(
                name: "IX_templates_indicators_TemplateDboId",
                table: "templates_indicators");

            migrationBuilder.DropColumn(
                name: "TemplateId",
                table: "templates_indicators");

            migrationBuilder.DropColumn(
                name: "TemplateDboId",
                table: "templates_indicators");

            migrationBuilder.RenameColumn(
                name: "Order",
                table: "templates_indicators",
                newName: "TemplatesId");

            migrationBuilder.RenameColumn(
                name: "IndicatorId",
                table: "templates_indicators",
                newName: "IndicatorsId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_templates_indicators",
                table: "templates_indicators",
                columns: new[] { "IndicatorsId", "TemplatesId" });

            migrationBuilder.CreateIndex(
                name: "IX_templates_indicators_TemplatesId",
                table: "templates_indicators",
                column: "TemplatesId");

            migrationBuilder.AddForeignKey(
                name: "FK_templates_indicators_indicators_IndicatorsId",
                table: "templates_indicators",
                column: "IndicatorsId",
                principalTable: "indicators",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_templates_indicators_templates_TemplatesId",
                table: "templates_indicators",
                column: "TemplatesId",
                principalTable: "templates",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
