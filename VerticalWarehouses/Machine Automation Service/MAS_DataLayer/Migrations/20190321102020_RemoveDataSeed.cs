using Microsoft.EntityFrameworkCore.Migrations;

namespace Ferretto.VW.MAS_DataLayer.Migrations
{
    public partial class RemoveDataSeed : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "ConfigurationValues",
                keyColumns: new[] { "CategoryName", "VarName" },
                keyValues: new object[] { 0L, 2L });

            migrationBuilder.DeleteData(
                table: "ConfigurationValues",
                keyColumns: new[] { "CategoryName", "VarName" },
                keyValues: new object[] { 0L, 4L });

            migrationBuilder.DeleteData(
                table: "ConfigurationValues",
                keyColumns: new[] { "CategoryName", "VarName" },
                keyValues: new object[] { 0L, 17L });

            migrationBuilder.DeleteData(
                table: "ConfigurationValues",
                keyColumns: new[] { "CategoryName", "VarName" },
                keyValues: new object[] { 0L, 19L });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "ConfigurationValues",
                columns: new[] { "CategoryName", "VarName", "VarType", "VarValue" },
                values: new object[] { 0L, 2L, 5L, "169.254.231.248" });

            migrationBuilder.InsertData(
                table: "ConfigurationValues",
                columns: new[] { "CategoryName", "VarName", "VarType", "VarValue" },
                values: new object[] { 0L, 17L, 2L, "17221" });

            migrationBuilder.InsertData(
                table: "ConfigurationValues",
                columns: new[] { "CategoryName", "VarName", "VarType", "VarValue" },
                values: new object[] { 0L, 4L, 5L, "169.254.231.10" });

            migrationBuilder.InsertData(
                table: "ConfigurationValues",
                columns: new[] { "CategoryName", "VarName", "VarType", "VarValue" },
                values: new object[] { 0L, 19L, 2L, "502" });
        }
    }
}
