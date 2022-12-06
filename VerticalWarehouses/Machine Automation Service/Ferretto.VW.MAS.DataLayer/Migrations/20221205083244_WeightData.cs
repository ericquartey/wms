using Microsoft.EntityFrameworkCore.Migrations;

namespace Ferretto.VW.MAS.DataLayer.Migrations
{
    public partial class WeightData : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WeightData",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Current = table.Column<double>(nullable: false),
                    LUTare = table.Column<double>(nullable: false),
                    NetWeight = table.Column<double>(nullable: false),
                    Step = table.Column<int>(nullable: false),
                    WeightMeasurementId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WeightData", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WeightData_WeightMeasurements_WeightMeasurementId",
                        column: x => x.WeightMeasurementId,
                        principalTable: "WeightMeasurements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WeightData_WeightMeasurementId",
                table: "WeightData",
                column: "WeightMeasurementId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WeightData");
        }
    }
}
