using Microsoft.EntityFrameworkCore.Migrations;

namespace Ferretto.VW.MAS.DataLayer.Migrations
{
    public partial class MaintainerNameandNote : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MaintainerName",
                table: "ServicingInfo",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Note",
                table: "ServicingInfo",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MaintainerName",
                table: "ServicingInfo");

            migrationBuilder.DropColumn(
                name: "Note",
                table: "ServicingInfo");
        }
    }
}
