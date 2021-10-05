using Microsoft.EntityFrameworkCore.Migrations;

namespace Ferretto.VW.MAS.DataLayer.Migrations
{
    public partial class horizontalResolutionProcedure : Migration
    {
        #region Methods

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HorizontalResolutionCalibrationId",
                table: "SetupProceduresSets");
        }

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "HorizontalResolutionCalibrationId",
                table: "SetupProceduresSets",
                nullable: true);

            migrationBuilder.Sql("INSERT INTO SetupProcedures (FeedRate, IsBypassed, IsCompleted, Discriminator) VALUES (1, 0, 0, 'SetupProcedure')");
            migrationBuilder.Sql("UPDATE SetupProceduresSets SET HorizontalResolutionCalibrationId = (SELECT MAX(ID) FROM SetupProcedures)");
        }

        #endregion
    }
}
