using Microsoft.EntityFrameworkCore.Migrations;

namespace Ferretto.VW.MAS.DataLayer.Migrations
{
    public partial class CalibrationCycles : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "HorizontalCyclesToCalibrate",
                table: "Machines",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "HorizontalPositionToCalibrate",
                table: "Machines",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "LastCalibrationCycles",
                table: "ElevatorAxes",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HorizontalCyclesToCalibrate",
                table: "Machines");

            migrationBuilder.DropColumn(
                name: "HorizontalPositionToCalibrate",
                table: "Machines");

            migrationBuilder.DropColumn(
                name: "LastCalibrationCycles",
                table: "ElevatorAxes");
        }
    }
}
