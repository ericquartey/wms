using Microsoft.EntityFrameworkCore.Migrations;

namespace Ferretto.VW.MAS.DataLayer.Migrations
{
    public partial class SecondTermMultiplier : Migration
    {
        #region Methods

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SecondTermMultiplier",
                table: "ElevatorStructuralProperties");
        }

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SecondTermMultiplier",
                table: "ElevatorStructuralProperties",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql("UPDATE ElevatorStructuralProperties SET SecondTermMultiplier = 1");
        }

        #endregion
    }
}
