using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Ferretto.VW.MAS.DataLayer.Migrations
{
    public partial class DeviceInfo : Migration
    {
        #region Methods

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Accessories_DeviceInformation_DeviceInformationId",
                table: "Accessories");

            migrationBuilder.DropTable(
                name: "DeviceInformation");

            migrationBuilder.DropIndex(
                name: "IX_Accessories_DeviceInformationId",
                table: "Accessories");

            migrationBuilder.DeleteData(
                table: "ErrorStatistics",
                keyColumn: "Code",
                keyValue: 75);

            migrationBuilder.DeleteData(
                table: "ErrorStatistics",
                keyColumn: "Code",
                keyValue: 76);

            migrationBuilder.DropColumn(
                name: "DeviceInformationId",
                table: "Accessories");
        }

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DeviceInformationId",
                table: "Accessories",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "DeviceInformation",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FirmwareVersion = table.Column<string>(nullable: true),
                    ManufactureDate = table.Column<DateTime>(nullable: true),
                    ModelNumber = table.Column<string>(nullable: true),
                    SerialNumber = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceInformation", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "ErrorStatistics",
                columns: new[] { "Code", "TotalErrors" },
                values: new object[] { 75, 0 });

            migrationBuilder.InsertData(
                table: "ErrorStatistics",
                columns: new[] { "Code", "TotalErrors" },
                values: new object[] { 76, 0 });

            migrationBuilder.CreateIndex(
                name: "IX_Accessories_DeviceInformationId",
                table: "Accessories",
                column: "DeviceInformationId");
        }

        #endregion
    }
}
