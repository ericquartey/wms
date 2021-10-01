using Microsoft.EntityFrameworkCore.Migrations;

namespace Ferretto.VW.TelemetryService.Migrations
{
    public partial class MissionLogFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "LoadUnitHeight",
                table: "MissionLogs",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "NetWeight",
                table: "MissionLogs",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LoadUnitHeight",
                table: "MissionLogs");

            migrationBuilder.DropColumn(
                name: "NetWeight",
                table: "MissionLogs");
        }
    }
}
