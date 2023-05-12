using Microsoft.EntityFrameworkCore.Migrations;

namespace Ferretto.VW.MAS.DataLayer.Migrations
{
    public partial class SpeaSensitiveAlarm : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsSpea",
                table: "Machines",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "SensitiveCarpetsAlarm",
                table: "Machines",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "SensitiveEdgeAlarm",
                table: "Machines",
                nullable: false,
                defaultValue: false);

            migrationBuilder.InsertData(
                table: "ErrorStatistics",
                columns: new[] { "Code", "TotalErrors" },
                values: new object[] { 96, 0 });

            migrationBuilder.InsertData(
                table: "ErrorStatistics",
                columns: new[] { "Code", "TotalErrors" },
                values: new object[] { 97, 0 });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "ErrorStatistics",
                keyColumn: "Code",
                keyValue: 96);

            migrationBuilder.DeleteData(
                table: "ErrorStatistics",
                keyColumn: "Code",
                keyValue: 97);

            migrationBuilder.DropColumn(
                name: "IsSpea",
                table: "Machines");

            migrationBuilder.DropColumn(
                name: "SensitiveCarpetsAlarm",
                table: "Machines");

            migrationBuilder.DropColumn(
                name: "SensitiveEdgeAlarm",
                table: "Machines");
        }
    }
}
