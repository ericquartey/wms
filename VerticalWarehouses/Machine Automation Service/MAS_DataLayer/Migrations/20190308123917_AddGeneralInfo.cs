using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Ferretto.VW.MAS_DataLayer.Migrations
{
    public partial class AddGeneralInfo : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "ConfigurationValues",
                keyColumn: "VarName",
                keyValue: 9L);

            migrationBuilder.DeleteData(
                table: "ConfigurationValues",
                keyColumn: "VarName",
                keyValue: 10L);

            migrationBuilder.DeleteData(
                table: "ConfigurationValues",
                keyColumn: "VarName",
                keyValue: 11L);

            migrationBuilder.DeleteData(
                table: "ConfigurationValues",
                keyColumn: "VarName",
                keyValue: 12L);

            migrationBuilder.CreateTable(
                name: "GeneralInfos",
                columns: table => new
                {
                    Address = table.Column<string>(nullable: true),
                    AlfaNum1 = table.Column<bool>(nullable: false),
                    AlfaNum2 = table.Column<bool>(nullable: false),
                    AlfaNum3 = table.Column<bool>(nullable: false),
                    Bays_Quantity = table.Column<int>(nullable: false),
                    City = table.Column<string>(nullable: true),
                    Client_Code = table.Column<string>(nullable: true),
                    Client_Name = table.Column<string>(nullable: true),
                    Country = table.Column<string>(nullable: true),
                    GeneralInfoId = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Height = table.Column<double>(nullable: false),
                    Height_Bay1 = table.Column<double>(nullable: false),
                    Height_Bay2 = table.Column<double>(nullable: false),
                    Height_Bay3 = table.Column<double>(nullable: false),
                    Installation_Date = table.Column<DateTime>(nullable: false),
                    Laser1 = table.Column<bool>(nullable: false),
                    Laser2 = table.Column<bool>(nullable: false),
                    Laser3 = table.Column<bool>(nullable: false),
                    Latitude = table.Column<string>(nullable: true),
                    Longitude = table.Column<string>(nullable: true),
                    Machine_Number_In_Area = table.Column<int>(nullable: false),
                    Model = table.Column<string>(nullable: true),
                    Order = table.Column<string>(nullable: true),
                    Position_Bay1 = table.Column<double>(nullable: false),
                    Position_Bay2 = table.Column<double>(nullable: false),
                    Position_Bay3 = table.Column<double>(nullable: false),
                    Province = table.Column<string>(nullable: true),
                    Serial = table.Column<string>(nullable: true),
                    Type_Bay1 = table.Column<int>(nullable: false),
                    Type_Bay2 = table.Column<int>(nullable: false),
                    Type_Bay3 = table.Column<int>(nullable: false),
                    Type_Shutter1 = table.Column<int>(nullable: false),
                    Type_Shutter2 = table.Column<int>(nullable: false),
                    Type_Shutter3 = table.Column<int>(nullable: false),
                    WMS_ON = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GeneralInfos", x => x.GeneralInfoId);
                });

            migrationBuilder.InsertData(
                table: "ConfigurationValues",
                columns: new[] { "VarName", "VarType", "VarValue" },
                values: new object[] { 9L, 3L, "169.254.231.248" });

            migrationBuilder.InsertData(
                table: "ConfigurationValues",
                columns: new[] { "VarName", "VarType", "VarValue" },
                values: new object[] { 10L, 0L, "17221" });

            migrationBuilder.InsertData(
                table: "ConfigurationValues",
                columns: new[] { "VarName", "VarType", "VarValue" },
                values: new object[] { 11L, 3L, "169.254.231.10" });

            migrationBuilder.InsertData(
                table: "ConfigurationValues",
                columns: new[] { "VarName", "VarType", "VarValue" },
                values: new object[] { 12L, 0L, "502" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GeneralInfos");

            migrationBuilder.DeleteData(
                table: "ConfigurationValues",
                keyColumn: "VarName",
                keyValue: 9L);

            migrationBuilder.DeleteData(
                table: "ConfigurationValues",
                keyColumn: "VarName",
                keyValue: 10L);

            migrationBuilder.DeleteData(
                table: "ConfigurationValues",
                keyColumn: "VarName",
                keyValue: 11L);

            migrationBuilder.DeleteData(
                table: "ConfigurationValues",
                keyColumn: "VarName",
                keyValue: 12L);

            migrationBuilder.InsertData(
                table: "ConfigurationValues",
                columns: new[] { "VarName", "VarType", "VarValue" },
                values: new object[] { 9L, 3L, "169.254.231.248" });

            migrationBuilder.InsertData(
                table: "ConfigurationValues",
                columns: new[] { "VarName", "VarType", "VarValue" },
                values: new object[] { 10L, 0L, "17221" });

            migrationBuilder.InsertData(
                table: "ConfigurationValues",
                columns: new[] { "VarName", "VarType", "VarValue" },
                values: new object[] { 11L, 3L, "169.254.231.10" });

            migrationBuilder.InsertData(
                table: "ConfigurationValues",
                columns: new[] { "VarName", "VarType", "VarValue" },
                values: new object[] { 12L, 0L, "502" });
        }
    }
}
