using Microsoft.EntityFrameworkCore.Migrations;

namespace Ferretto.VW.MAS_DataLayer.Migrations
{
    public partial class AddDrawerIdToCell : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "ConfigurationValues",
                keyColumn: "VarName",
                keyValue: 8L);

            migrationBuilder.DeleteData(
                table: "ConfigurationValues",
                keyColumn: "VarName",
                keyValue: 9L);

            migrationBuilder.AddColumn<int>(
                name: "DrawerId",
                table: "Cells",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.InsertData(
                table: "ConfigurationValues",
                columns: new[] { "VarName", "VarType", "VarValue" },
                values: new object[] { 8L, 3L, "169.254.231.248" });

            migrationBuilder.InsertData(
                table: "ConfigurationValues",
                columns: new[] { "VarName", "VarType", "VarValue" },
                values: new object[] { 9L, 0L, "17221" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "ConfigurationValues",
                keyColumn: "VarName",
                keyValue: 8L);

            migrationBuilder.DeleteData(
                table: "ConfigurationValues",
                keyColumn: "VarName",
                keyValue: 9L);

            migrationBuilder.DropColumn(
                name: "DrawerId",
                table: "Cells");

            migrationBuilder.InsertData(
                table: "ConfigurationValues",
                columns: new[] { "VarName", "VarType", "VarValue" },
                values: new object[] { 8L, 3L, "169.254.231.248" });

            migrationBuilder.InsertData(
                table: "ConfigurationValues",
                columns: new[] { "VarName", "VarType", "VarValue" },
                values: new object[] { 9L, 0L, "17221" });
        }
    }
}
