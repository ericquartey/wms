using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Ferretto.VW.MAS.DataLayer.Migrations
{
    public partial class initialcreation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "Resolution",
                table: "Bays",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.UpdateData(
                table: "ErrorDefinitions",
                keyColumn: "Id",
                keyValue: 200015,
                column: "Description",
                value: "Errore sconosciuto 15 dell'inverter.");

            migrationBuilder.InsertData(
                table: "ErrorDefinitions",
                columns: new[] { "Id", "Code", "Description", "Reason", "Severity" },
                values: new object[] { 200016, 200016, "InverterErrorUnknown16", "InverterErrorUnknown16", 1 });

            migrationBuilder.InsertData(
                table: "ErrorDefinitions",
                columns: new[] { "Id", "Code", "Description", "Reason", "Severity" },
                values: new object[] { 200017, 200017, "InverterErrorUnknown17", "InverterErrorUnknown17", 1 });

            migrationBuilder.InsertData(
                table: "ErrorDefinitions",
                columns: new[] { "Id", "Code", "Description", "Reason", "Severity" },
                values: new object[] { 200018, 200018, "InverterErrorUnknown18", "InverterErrorUnknown18", 1 });

            migrationBuilder.InsertData(
                table: "ErrorDefinitions",
                columns: new[] { "Id", "Code", "Description", "Reason", "Severity" },
                values: new object[] { 200019, 200019, "InverterErrorUnknown19", "InverterErrorUnknown19", 1 });

            migrationBuilder.UpdateData(
                table: "ServicingInfo",
                keyColumn: "Id",
                keyValue: 1,
                column: "InstallationDate",
                value: new DateTime(2016, 12, 17, 19, 24, 45, 21, DateTimeKind.Local).AddTicks(4314));

            migrationBuilder.InsertData(
                table: "ErrorStatistics",
                columns: new[] { "Code", "TotalErrors" },
                values: new object[] { 200016, 0 });

            migrationBuilder.InsertData(
                table: "ErrorStatistics",
                columns: new[] { "Code", "TotalErrors" },
                values: new object[] { 200017, 0 });

            migrationBuilder.InsertData(
                table: "ErrorStatistics",
                columns: new[] { "Code", "TotalErrors" },
                values: new object[] { 200018, 0 });

            migrationBuilder.InsertData(
                table: "ErrorStatistics",
                columns: new[] { "Code", "TotalErrors" },
                values: new object[] { 200019, 0 });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "ErrorStatistics",
                keyColumn: "Code",
                keyValue: 200016);

            migrationBuilder.DeleteData(
                table: "ErrorStatistics",
                keyColumn: "Code",
                keyValue: 200017);

            migrationBuilder.DeleteData(
                table: "ErrorStatistics",
                keyColumn: "Code",
                keyValue: 200018);

            migrationBuilder.DeleteData(
                table: "ErrorStatistics",
                keyColumn: "Code",
                keyValue: 200019);

            migrationBuilder.DeleteData(
                table: "ErrorDefinitions",
                keyColumn: "Id",
                keyValue: 200016);

            migrationBuilder.DeleteData(
                table: "ErrorDefinitions",
                keyColumn: "Id",
                keyValue: 200017);

            migrationBuilder.DeleteData(
                table: "ErrorDefinitions",
                keyColumn: "Id",
                keyValue: 200018);

            migrationBuilder.DeleteData(
                table: "ErrorDefinitions",
                keyColumn: "Id",
                keyValue: 200019);

            migrationBuilder.DropColumn(
                name: "Resolution",
                table: "Bays");

            migrationBuilder.UpdateData(
                table: "ErrorDefinitions",
                keyColumn: "Id",
                keyValue: 200015,
                column: "Description",
                value: "Errore sconosciuto dell'inverter.");

            migrationBuilder.UpdateData(
                table: "ServicingInfo",
                keyColumn: "Id",
                keyValue: 1,
                column: "InstallationDate",
                value: new DateTime(2016, 12, 17, 16, 48, 46, 117, DateTimeKind.Local).AddTicks(8416));
        }
    }
}
