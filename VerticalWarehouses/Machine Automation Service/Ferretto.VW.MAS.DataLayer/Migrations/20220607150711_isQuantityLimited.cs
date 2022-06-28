using Microsoft.EntityFrameworkCore.Migrations;

namespace Ferretto.VW.MAS.DataLayer.Migrations
{
    public partial class isQuantityLimited : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsQuantityLimited",
                table: "Machines",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsQuantityLimited",
                table: "Machines");
        }
    }
}
