using Microsoft.EntityFrameworkCore.Migrations;

namespace Ferretto.VW.MAS.DataLayer.Migrations
{
    public partial class shutterParams : Migration
    {
        #region Methods

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("delete from ErrorStatistics");

            migrationBuilder.DeleteData(
                table: "ErrorStatistics",
                keyColumn: "Code",
                keyValue: 69);

            migrationBuilder.DropColumn(
                name: "HighSpeedHalfDurationClose",
                table: "ShutterManualParameters");

            migrationBuilder.DropColumn(
                name: "HighSpeedHalfDurationOpen",
                table: "ShutterManualParameters");
        }

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "HighSpeedHalfDurationClose",
                table: "ShutterManualParameters",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "HighSpeedHalfDurationOpen",
                table: "ShutterManualParameters",
                nullable: true);

            migrationBuilder.Sql("delete from ErrorStatistics");

            migrationBuilder.InsertData(
                table: "ErrorStatistics",
                columns: new[] { "Code", "TotalErrors" },
                values: new object[] { 69, 0 });
        }

        #endregion
    }
}
