using Microsoft.EntityFrameworkCore.Migrations;

namespace Ferretto.VW.MAS.DataLayer.Migrations
{
    public partial class HomingAcceleration : Migration
    {
        #region Methods

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HomingAcceleration",
                table: "ElevatorAxes");
        }

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "HomingAcceleration",
                table: "ElevatorAxes",
                nullable: false,
                defaultValue: 100.0);

            migrationBuilder.Sql("UPDATE ElevatorAxes SET HomingAcceleration = 100");
        }

        #endregion
    }
}
