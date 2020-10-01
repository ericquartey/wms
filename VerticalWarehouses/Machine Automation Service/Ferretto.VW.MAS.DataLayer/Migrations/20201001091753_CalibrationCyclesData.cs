using Microsoft.EntityFrameworkCore.Migrations;

namespace Ferretto.VW.MAS.DataLayer.Migrations
{
    public partial class CalibrationCyclesData : Migration
    {
        #region Methods

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CyclesToCalibrate",
                table: "Bays");

            migrationBuilder.DropColumn(
                name: "LastCalibrationCycles",
                table: "Bays");
        }

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CyclesToCalibrate",
                table: "Bays",
                nullable: false,
                defaultValue: 50);

            migrationBuilder.AddColumn<int>(
                name: "LastCalibrationCycles",
                table: "Bays",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql("UPDATE Bays SET CyclesToCalibrate = 50");
        }

        #endregion
    }
}
