using Microsoft.EntityFrameworkCore.Migrations;

namespace Ferretto.VW.MAS.DataLayer.Migrations
{
    public partial class InverterParameter : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Value",
                table: "InverterParameter",
                newName: "WriteCode");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "InverterParameter",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ReadCode",
                table: "InverterParameter",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "InverterParameter");

            migrationBuilder.DropColumn(
                name: "ReadCode",
                table: "InverterParameter");

            migrationBuilder.RenameColumn(
                name: "WriteCode",
                table: "InverterParameter",
                newName: "Value");
        }
    }
}
