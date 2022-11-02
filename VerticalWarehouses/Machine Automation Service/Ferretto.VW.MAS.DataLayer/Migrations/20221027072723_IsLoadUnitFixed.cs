using Microsoft.EntityFrameworkCore.Migrations;

namespace Ferretto.VW.MAS.DataLayer.Migrations
{
    public partial class IsLoadUnitFixed : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsLoadUnitFixed",
                table: "Machines",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsCellFixed",
                table: "LoadingUnits",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsHeightFixed",
                table: "LoadingUnits",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsLoadUnitFixed",
                table: "Machines");

            migrationBuilder.DropColumn(
                name: "IsCellFixed",
                table: "LoadingUnits");

            migrationBuilder.DropColumn(
                name: "IsHeightFixed",
                table: "LoadingUnits");
        }
    }
}
