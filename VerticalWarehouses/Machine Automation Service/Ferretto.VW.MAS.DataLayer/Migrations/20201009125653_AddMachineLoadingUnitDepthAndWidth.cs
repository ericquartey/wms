using Microsoft.EntityFrameworkCore.Migrations;

namespace Ferretto.VW.MAS.DataLayer.Migrations
{
    public partial class AddMachineLoadingUnitDepthAndWidth : Migration
    {
        #region Methods

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LoadUnitDepth",
                table: "Machines");

            migrationBuilder.DropColumn(
                name: "LoadUnitWidth",
                table: "Machines");
        }

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "LoadUnitDepth",
                table: "Machines",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "LoadUnitWidth",
                table: "Machines",
                nullable: false,
                defaultValue: 0.0);
        }

        #endregion
    }
}
