using Microsoft.EntityFrameworkCore.Migrations;

namespace Ferretto.VW.MAS.DataLayer.Migrations
{
    public partial class LoadFirstDrawerTest_To_PositioningProcedure : Migration
    {
        #region Methods

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "ErrorStatistics",
                keyColumn: "Code",
                keyValue: 67);

            migrationBuilder.DeleteData(
                table: "ErrorStatistics",
                keyColumn: "Code",
                keyValue: 68);

            migrationBuilder.Sql("UPDATE SetupProcedures SET Discriminator = 'SetupProcedure' WHERE Id IN (SELECT LoadFirstDrawerTestId FROM SetupProceduresSets)");
        }

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "ErrorStatistics",
                columns: new[] { "Code", "TotalErrors" },
                values: new object[] { 67, 0 });

            migrationBuilder.InsertData(
                table: "ErrorStatistics",
                columns: new[] { "Code", "TotalErrors" },
                values: new object[] { 68, 0 });

            migrationBuilder.Sql("UPDATE SetupProcedures SET Discriminator = 'PositioningProcedure', InProgress = 0, Step = 0 WHERE Id IN (SELECT LoadFirstDrawerTestId FROM SetupProceduresSets)");
        }

        #endregion
    }
}
