using Microsoft.EntityFrameworkCore.Migrations;

namespace Ferretto.VW.MAS.DataLayer.Migrations
{
    public partial class HorizontalCalibrateSpeed : Migration
    {
        #region Methods

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HorizontalCalibrateSpeed",
                table: "ElevatorAxes");
        }

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "HorizontalCalibrateSpeed",
                table: "ElevatorAxes",
                nullable: false,
                defaultValue: 1.5);

            migrationBuilder.Sql("UPDATE ElevatorAxes SET HorizontalCalibrateSpeed = 1.5");
        }

        #endregion
    }
}
