using Microsoft.EntityFrameworkCore.Migrations;

namespace Ferretto.VW.MAS.DataLayer.Migrations
{
    public partial class WaitingListPriorityHighlightedAndListConfirm : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ListPickConfirm",
                table: "Machines",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ListPutConfirm",
                table: "Machines",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "WaitingListPriorityHighlighted",
                table: "Machines",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ListPickConfirm",
                table: "Machines");

            migrationBuilder.DropColumn(
                name: "ListPutConfirm",
                table: "Machines");

            migrationBuilder.DropColumn(
                name: "WaitingListPriorityHighlighted",
                table: "Machines");
        }
    }
}
