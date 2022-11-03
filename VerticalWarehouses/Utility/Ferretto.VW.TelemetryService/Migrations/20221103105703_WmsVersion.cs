using Microsoft.EntityFrameworkCore.Migrations;

namespace Ferretto.VW.TelemetryService.Migrations
{
    public partial class WmsVersion : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "WmsVersion",
                table: "Machines",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "WmsVersion",
                table: "Machines");
        }
    }
}
