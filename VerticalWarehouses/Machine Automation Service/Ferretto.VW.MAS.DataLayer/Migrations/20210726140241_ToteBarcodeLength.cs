using Microsoft.EntityFrameworkCore.Migrations;

namespace Ferretto.VW.MAS.DataLayer.Migrations
{
    public partial class ToteBarcodeLength : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ToteBarcodeLength",
                table: "Machines",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.InsertData(
                table: "ErrorStatistics",
                columns: new[] { "Code", "TotalErrors" },
                values: new object[] { 87, 0 });

            migrationBuilder.InsertData(
                table: "ErrorStatistics",
                columns: new[] { "Code", "TotalErrors" },
                values: new object[] { 88, 0 });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "ErrorStatistics",
                keyColumn: "Code",
                keyValue: 87);

            migrationBuilder.DeleteData(
                table: "ErrorStatistics",
                keyColumn: "Code",
                keyValue: 88);

            migrationBuilder.DropColumn(
                name: "ToteBarcodeLength",
                table: "Machines");
        }
    }
}
