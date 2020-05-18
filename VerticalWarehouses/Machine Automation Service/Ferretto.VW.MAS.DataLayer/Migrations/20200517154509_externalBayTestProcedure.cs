using Microsoft.EntityFrameworkCore.Migrations;

namespace Ferretto.VW.MAS.DataLayer.Migrations
{
    public partial class externalBayTestProcedure : Migration
    {
        #region Methods

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Bay1ExternalCalibrationId",
                table: "SetupProceduresSets");

            migrationBuilder.DropColumn(
                name: "Bay2ExternalCalibrationId",
                table: "SetupProceduresSets");

            migrationBuilder.DropColumn(
                name: "Bay3ExternalCalibrationId",
                table: "SetupProceduresSets");

            migrationBuilder.Sql("DELETE FROM SetupProcedures WHERE id = (SELECT MAX(ID) FROM SetupProcedures)");
            migrationBuilder.Sql("DELETE FROM SetupProcedures WHERE id = (SELECT MAX(ID) FROM SetupProcedures)");
            migrationBuilder.Sql("DELETE FROM SetupProcedures WHERE id = (SELECT MAX(ID) FROM SetupProcedures)");
        }

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Bay1ExternalCalibrationId",
                table: "SetupProceduresSets",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Bay2ExternalCalibrationId",
                table: "SetupProceduresSets",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Bay3ExternalCalibrationId",
                table: "SetupProceduresSets",
                nullable: true);

            migrationBuilder.Sql("INSERT INTO SetupProcedures (FeedRate, IsBypassed, IsCompleted, RepeatedTestProcedure_InProgress, performedCycles, requiredCycles, Discriminator) VALUES (1, 0, 0, 0, 0, 6, 'RepeatedTestProcedure')");
            migrationBuilder.Sql("UPDATE SetupProceduresSets SET Bay1ExternalCalibrationId = (SELECT MAX(ID) FROM SetupProcedures)");
            migrationBuilder.Sql("INSERT INTO SetupProcedures (FeedRate, IsBypassed, IsCompleted, RepeatedTestProcedure_InProgress, performedCycles, requiredCycles, Discriminator) VALUES (1, 0, 0, 0, 0, 6, 'RepeatedTestProcedure')");
            migrationBuilder.Sql("UPDATE SetupProceduresSets SET Bay2ExternalCalibrationId = (SELECT MAX(ID) FROM SetupProcedures)");
            migrationBuilder.Sql("INSERT INTO SetupProcedures (FeedRate, IsBypassed, IsCompleted, RepeatedTestProcedure_InProgress, performedCycles, requiredCycles, Discriminator) VALUES (1, 0, 0, 0, 0, 6, 'RepeatedTestProcedure')");
            migrationBuilder.Sql("UPDATE SetupProceduresSets SET Bay3ExternalCalibrationId = (SELECT MAX(ID) FROM SetupProcedures)");
        }

        #endregion
    }
}
