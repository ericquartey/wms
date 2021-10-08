using Microsoft.EntityFrameworkCore.Migrations;

namespace Ferretto.VW.MAS.DataLayer.Migrations
{
    public partial class horizontalResolutionProcedure : Migration
    {
        #region Methods

        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "HorizontalResolutionCalibrationId",
                table: "SetupProceduresSets",
                nullable: true);

            migrationBuilder.Sql("INSERT INTO SetupProcedures (FeedRate, IsBypassed, IsCompleted, RepeatedTestProcedure_InProgress, performedCycles, requiredCycles, Discriminator) VALUES (1, 0, 0, 0, 0, 50, 'RepeatedTestProcedure')");
            migrationBuilder.Sql("UPDATE SetupProceduresSets SET HorizontalResolutionCalibrationId = (SELECT MAX(ID) FROM SetupProcedures)");
        }

        #endregion
    }
}
