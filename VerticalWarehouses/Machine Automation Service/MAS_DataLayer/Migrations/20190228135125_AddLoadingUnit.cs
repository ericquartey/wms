using Microsoft.EntityFrameworkCore.Migrations;

namespace Ferretto.VW.MAS_DataLayer.Migrations
{
    public partial class AddLoadingUnit : Migration
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

            migrationBuilder.CreateTable(
                name: "LoadingUnits",
                columns: table => new
                {
                    CellPosition = table.Column<decimal>(nullable: false),
                    Height = table.Column<decimal>(nullable: false),
                    LoadingUnitId = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Status = table.Column<long>(nullable: false),
                    Weight = table.Column<decimal>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoadingUnits", x => x.LoadingUnitId);
                });

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
            migrationBuilder.DropTable(
                name: "LoadingUnits");

            migrationBuilder.DeleteData(
                table: "ConfigurationValues",
                keyColumn: "VarName",
                keyValue: 8L);

            migrationBuilder.DeleteData(
                table: "ConfigurationValues",
                keyColumn: "VarName",
                keyValue: 9L);

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
