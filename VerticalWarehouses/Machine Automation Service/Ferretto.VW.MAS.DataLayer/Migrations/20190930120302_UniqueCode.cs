using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Ferretto.VW.MAS.DataLayer.Migrations
{
    public partial class UniqueCode : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Errors_Code",
                table: "Errors");

            migrationBuilder.UpdateData(
                table: "ServicingInfo",
                keyColumn: "Id",
                keyValue: 1,
                column: "InstallationDate",
                value: new DateTime(2016, 11, 30, 14, 3, 0, 787, DateTimeKind.Local).AddTicks(6118));

            migrationBuilder.CreateIndex(
                name: "IX_Errors_Code",
                table: "Errors",
                column: "Code");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Errors_Code",
                table: "Errors");

            migrationBuilder.UpdateData(
                table: "ServicingInfo",
                keyColumn: "Id",
                keyValue: 1,
                column: "InstallationDate",
                value: new DateTime(2016, 11, 30, 11, 5, 42, 62, DateTimeKind.Local).AddTicks(2011));

            migrationBuilder.CreateIndex(
                name: "IX_Errors_Code",
                table: "Errors",
                column: "Code",
                unique: true);
        }
    }
}
