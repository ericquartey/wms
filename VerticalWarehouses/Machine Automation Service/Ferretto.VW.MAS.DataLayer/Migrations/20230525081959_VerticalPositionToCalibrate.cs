using Microsoft.EntityFrameworkCore.Migrations;

namespace Ferretto.VW.MAS.DataLayer.Migrations
{
    public partial class VerticalPositionToCalibrate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "VerticalPositionToCalibrate",
                table: "Machines",
                nullable: false,
                defaultValue: 1000);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "VerticalPositionToCalibrate",
                table: "Machines");
        }
    }
}
