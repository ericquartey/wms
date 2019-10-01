using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Ferretto.VW.MAS.DataLayer.Migrations
{
    public partial class BaySide : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Side",
                table: "Bays",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "ServicingInfo",
                keyColumn: "Id",
                keyValue: 1,
                column: "InstallationDate",
                value: new DateTime(2016, 11, 30, 11, 5, 42, 62, DateTimeKind.Local).AddTicks(2011));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Side",
                table: "Bays");

            migrationBuilder.UpdateData(
                table: "ServicingInfo",
                keyColumn: "Id",
                keyValue: 1,
                column: "InstallationDate",
                value: new DateTime(2016, 11, 27, 10, 11, 36, 636, DateTimeKind.Local).AddTicks(4663));
        }
    }
}
