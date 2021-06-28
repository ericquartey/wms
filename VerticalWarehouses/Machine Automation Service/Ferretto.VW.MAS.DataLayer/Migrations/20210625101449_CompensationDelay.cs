using Microsoft.EntityFrameworkCore.Migrations;

namespace Ferretto.VW.MAS.DataLayer.Migrations
{
    public partial class CompensationDelay : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "VerticalDepositCompensationDelay",
                table: "ElevatorAxes",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "VerticalPickupCompensationDelay",
                table: "ElevatorAxes",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "VerticalDepositCompensationDelay",
                table: "ElevatorAxes");

            migrationBuilder.DropColumn(
                name: "VerticalPickupCompensationDelay",
                table: "ElevatorAxes");
        }
    }
}
