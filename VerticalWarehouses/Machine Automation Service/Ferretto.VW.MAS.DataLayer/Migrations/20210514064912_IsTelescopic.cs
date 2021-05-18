using Microsoft.EntityFrameworkCore.Migrations;

namespace Ferretto.VW.MAS.DataLayer.Migrations
{
    public partial class IsTelescopic : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsTelescopic",
                table: "Bays",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsTelescopic",
                table: "Bays");
        }
    }
}
