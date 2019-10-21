using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Ferretto.VW.MAS.DataLayer.Migrations
{
    public partial class initialcreation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "ErrorStatistics",
                keyColumn: "Code",
                keyValue: 200015);

            migrationBuilder.DeleteData(
                table: "ErrorDefinitions",
                keyColumn: "Id",
                keyValue: 200015);

            migrationBuilder.AddColumn<double>(
                name: "WeightMeasureMultiply",
                table: "ElevatorAxes",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "WeightMeasureSpeed",
                table: "ElevatorAxes",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "WeightMeasureSum",
                table: "ElevatorAxes",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "Resolution",
                table: "Bays",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.UpdateData(
                table: "ServicingInfo",
                keyColumn: "Id",
                keyValue: 1,
                column: "InstallationDate",
                value: new DateTime(2016, 12, 21, 10, 50, 30, 492, DateTimeKind.Local).AddTicks(6894));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "WeightMeasureMultiply",
                table: "ElevatorAxes");

            migrationBuilder.DropColumn(
                name: "WeightMeasureSpeed",
                table: "ElevatorAxes");

            migrationBuilder.DropColumn(
                name: "WeightMeasureSum",
                table: "ElevatorAxes");

            migrationBuilder.DropColumn(
                name: "Resolution",
                table: "Bays");

            migrationBuilder.InsertData(
                table: "ErrorDefinitions",
                columns: new[] { "Id", "Code", "Description", "Reason", "Severity" },
                values: new object[] { 200015, 200015, "Errore sconosciuto dell'inverter.", "Spegnere e riaccendere la macchina. Se il problema persiste, contattare l'assistenza.", 1 });

            migrationBuilder.UpdateData(
                table: "ServicingInfo",
                keyColumn: "Id",
                keyValue: 1,
                column: "InstallationDate",
                value: new DateTime(2016, 12, 17, 16, 48, 46, 117, DateTimeKind.Local).AddTicks(8416));

            migrationBuilder.InsertData(
                table: "ErrorStatistics",
                columns: new[] { "Code", "TotalErrors" },
                values: new object[] { 200015, 0 });
        }
    }
}
