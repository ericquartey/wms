using Microsoft.EntityFrameworkCore.Migrations;

namespace Ferretto.VW.TelemetryService.Migrations
{
    public partial class ErrorId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ErrorId",
                table: "ErrorLogs",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ErrorId",
                table: "ErrorLogs");
        }
    }
}
