using Microsoft.EntityFrameworkCore.Migrations;

namespace Ferretto.VW.MAS.DataLayer.Migrations
{
    public partial class LUFixed2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FixedCell",
                table: "LoadingUnits",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "FixedHeight",
                table: "LoadingUnits",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FixedCell",
                table: "LoadingUnits");

            migrationBuilder.DropColumn(
                name: "FixedHeight",
                table: "LoadingUnits");
        }
    }
}
