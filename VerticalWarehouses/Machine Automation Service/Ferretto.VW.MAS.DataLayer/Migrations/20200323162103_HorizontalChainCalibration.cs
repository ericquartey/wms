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

            migrationBuilder.Sql("DELETE FROM SetupProcedures WHERE id = (SELECT MAX(ID) FROM SetupProcedures)");
        }

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "HorizontalChainCalibrationId",
                table: "SetupProceduresSets",
                nullable: true);

            migrationBuilder.Sql("INSERT INTO SetupProcedures (FeedRate, IsBypassed, IsCompleted, Discriminator) VALUES (1, 0, 0, 'SetupProcedure')");
            migrationBuilder.Sql("UPDATE SetupProceduresSets SET HorizontalChainCalibrationId = (SELECT MAX(ID) FROM SetupProcedures)");
        }

        #endregion
    }
}
