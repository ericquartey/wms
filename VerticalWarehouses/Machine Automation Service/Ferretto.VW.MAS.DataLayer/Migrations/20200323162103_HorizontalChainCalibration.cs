using Microsoft.EntityFrameworkCore.Migrations;

namespace Ferretto.VW.MAS.DataLayer.Migrations
{
    public partial class HorizontalChainCalibration : Migration
    {
        #region Methods

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HorizontalChainCalibrationId",
                table: "SetupProceduresSets");
        }

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "HorizontalChainCalibrationId",
                table: "SetupProceduresSets",
                nullable: true);
        }

        #endregion
    }
}
