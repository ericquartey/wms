using Microsoft.EntityFrameworkCore.Migrations;

namespace Ferretto.VW.MAS.DataLayer.Migrations
{
    public partial class AccessoryColumns : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "StringValue",
                table: "InverterParameter",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsConfiguredNew",
                table: "Accessories",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsEnabledNew",
                table: "Accessories",
                nullable: false,
                defaultValue: false);

            migrationBuilder.InsertData(
                table: "ErrorStatistics",
                columns: new[] { "Code", "TotalErrors" },
                values: new object[] { 77, 0 });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "ErrorStatistics",
                keyColumn: "Code",
                keyValue: 77);

            migrationBuilder.DropColumn(
                name: "StringValue",
                table: "InverterParameter");

            migrationBuilder.DropColumn(
                name: "IsConfiguredNew",
                table: "Accessories");

            migrationBuilder.DropColumn(
                name: "IsEnabledNew",
                table: "Accessories");
        }
    }
}
