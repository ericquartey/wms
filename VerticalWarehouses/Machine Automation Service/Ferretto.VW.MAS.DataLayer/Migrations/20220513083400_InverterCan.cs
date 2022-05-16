using Microsoft.EntityFrameworkCore.Migrations;

namespace Ferretto.VW.MAS.DataLayer.Migrations
{
    public partial class InverterCan : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CanOpenNode",
                table: "Inverters",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsEthernetIP",
                table: "Inverters",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CanOpenNode",
                table: "Inverters");

            migrationBuilder.DropColumn(
                name: "IsEthernetIP",
                table: "Inverters");
        }
    }
}
