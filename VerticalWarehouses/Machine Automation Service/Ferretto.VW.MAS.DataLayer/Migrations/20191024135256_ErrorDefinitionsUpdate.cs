using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Ferretto.VW.MAS.DataLayer.Migrations
{
    public partial class ErrorDefinitionsUpdate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "ErrorDefinitions",
                columns: new[] { "Id", "Code", "Description", "Reason", "Severity" },
                values: new object[] { 300000, 300000, "Errore Machine Manager", "Spegnere e riaccendere la macchina. Se il problema persiste, contattare l'assistenza.", 1 });

            migrationBuilder.InsertData(
                table: "ErrorDefinitions",
                columns: new[] { "Id", "Code", "Description", "Reason", "Severity" },
                values: new object[] { 300001, 300001, "Nessun cassetto presente nella baia indicata", "Assicurarsi che un cassetto sia presente in baia e che i sensori di presenza funzionino correttamente", 1 });

            migrationBuilder.InsertData(
                table: "ErrorDefinitions",
                columns: new[] { "Id", "Code", "Description", "Reason", "Severity" },
                values: new object[] { 300002, 300002, "Inconsistenza database posizione sorgente cassetto", "Verificare che la posizione sorgente del cassetto all'interno del database sia correttamente configurata", 1 });

            migrationBuilder.InsertData(
                table: "ErrorDefinitions",
                columns: new[] { "Id", "Code", "Description", "Reason", "Severity" },
                values: new object[] { 300003, 300003, "Inconsistenza database posizione destinazione cassetto", "Verificare che la posizione destinazione del cassetto all'interno del database sia correttamente configurata", 1 });

            migrationBuilder.InsertData(
                table: "ErrorDefinitions",
                columns: new[] { "Id", "Code", "Description", "Reason", "Severity" },
                values: new object[] { 300004, 300004, "", "", 1 });

            migrationBuilder.UpdateData(
                table: "ServicingInfo",
                keyColumn: "Id",
                keyValue: 1,
                column: "InstallationDate",
                value: new DateTime(2016, 12, 24, 15, 52, 56, 142, DateTimeKind.Local).AddTicks(6936));

            migrationBuilder.InsertData(
                table: "ErrorStatistics",
                columns: new[] { "Code", "TotalErrors" },
                values: new object[] { 300000, 0 });

            migrationBuilder.InsertData(
                table: "ErrorStatistics",
                columns: new[] { "Code", "TotalErrors" },
                values: new object[] { 300001, 0 });

            migrationBuilder.InsertData(
                table: "ErrorStatistics",
                columns: new[] { "Code", "TotalErrors" },
                values: new object[] { 300002, 0 });

            migrationBuilder.InsertData(
                table: "ErrorStatistics",
                columns: new[] { "Code", "TotalErrors" },
                values: new object[] { 300003, 0 });

            migrationBuilder.InsertData(
                table: "ErrorStatistics",
                columns: new[] { "Code", "TotalErrors" },
                values: new object[] { 300004, 0 });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "ErrorStatistics",
                keyColumn: "Code",
                keyValue: 300000);

            migrationBuilder.DeleteData(
                table: "ErrorStatistics",
                keyColumn: "Code",
                keyValue: 300001);

            migrationBuilder.DeleteData(
                table: "ErrorStatistics",
                keyColumn: "Code",
                keyValue: 300002);

            migrationBuilder.DeleteData(
                table: "ErrorStatistics",
                keyColumn: "Code",
                keyValue: 300003);

            migrationBuilder.DeleteData(
                table: "ErrorStatistics",
                keyColumn: "Code",
                keyValue: 300004);

            migrationBuilder.DeleteData(
                table: "ErrorDefinitions",
                keyColumn: "Id",
                keyValue: 300000);

            migrationBuilder.DeleteData(
                table: "ErrorDefinitions",
                keyColumn: "Id",
                keyValue: 300001);

            migrationBuilder.DeleteData(
                table: "ErrorDefinitions",
                keyColumn: "Id",
                keyValue: 300002);

            migrationBuilder.DeleteData(
                table: "ErrorDefinitions",
                keyColumn: "Id",
                keyValue: 300003);

            migrationBuilder.DeleteData(
                table: "ErrorDefinitions",
                keyColumn: "Id",
                keyValue: 300004);

            migrationBuilder.UpdateData(
                table: "ServicingInfo",
                keyColumn: "Id",
                keyValue: 1,
                column: "InstallationDate",
                value: new DateTime(2016, 12, 23, 17, 34, 31, 15, DateTimeKind.Local).AddTicks(9170));
        }
    }
}
