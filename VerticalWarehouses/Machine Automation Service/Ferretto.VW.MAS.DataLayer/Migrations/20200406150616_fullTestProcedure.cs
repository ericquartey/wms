using Microsoft.EntityFrameworkCore.Migrations;

namespace Ferretto.VW.MAS.DataLayer.Migrations
{
    public partial class fullTestProcedure : Migration
    {
        #region Methods

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Bay1FullTestId",
                table: "SetupProceduresSets");

            migrationBuilder.DropColumn(
                name: "Bay2FullTestId",
                table: "SetupProceduresSets");

            migrationBuilder.DropColumn(
                name: "Bay3FullTestId",
                table: "SetupProceduresSets");
            migrationBuilder.Sql("DELETE FROM SetupProcedures WHERE id = (SELECT MAX(ID) FROM SetupProcedures)");
            migrationBuilder.Sql("DELETE FROM SetupProcedures WHERE id = (SELECT MAX(ID) FROM SetupProcedures)");
            migrationBuilder.Sql("DELETE FROM SetupProcedures WHERE id = (SELECT MAX(ID) FROM SetupProcedures)");
        }

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Bay1FullTestId",
                table: "SetupProceduresSets",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Bay2FullTestId",
                table: "SetupProceduresSets",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Bay3FullTestId",
                table: "SetupProceduresSets",
                nullable: true);

            migrationBuilder.Sql("INSERT INTO SetupProcedures (FeedRate, IsBypassed, IsCompleted, RepeatedTestProcedure_InProgress, performedCycles, requiredCycles, Discriminator) VALUES (1, 0, 0, 0, 0, 0, 'RepeatedTestProcedure')");
            migrationBuilder.Sql("UPDATE SetupProceduresSets SET Bay1FullTestId = (SELECT MAX(ID) FROM SetupProcedures)");
            migrationBuilder.Sql("INSERT INTO SetupProcedures (FeedRate, IsBypassed, IsCompleted, RepeatedTestProcedure_InProgress, performedCycles, requiredCycles, Discriminator) VALUES (1, 0, 0, 0, 0, 0, 'RepeatedTestProcedure')");
            migrationBuilder.Sql("UPDATE SetupProceduresSets SET Bay2FullTestId = (SELECT MAX(ID) FROM SetupProcedures)");
            migrationBuilder.Sql("INSERT INTO SetupProcedures (FeedRate, IsBypassed, IsCompleted, RepeatedTestProcedure_InProgress, performedCycles, requiredCycles, Discriminator) VALUES (1, 0, 0, 0, 0, 0, 'RepeatedTestProcedure')");
            migrationBuilder.Sql("UPDATE SetupProceduresSets SET Bay3FullTestId = (SELECT MAX(ID) FROM SetupProcedures)");
        }

        #endregion
    }
}
