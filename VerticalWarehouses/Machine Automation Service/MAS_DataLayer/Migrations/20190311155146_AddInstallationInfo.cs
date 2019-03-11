using Microsoft.EntityFrameworkCore.Migrations;

namespace Ferretto.VW.MAS_DataLayer.Migrations
{
    public partial class AddInstallationInfo : Migration
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
                name: "InstallationInfos",
                columns: table => new
                {
                    Belt_Burnishing = table.Column<bool>(nullable: false),
                    InstallationInfoId = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Machine_Ok = table.Column<bool>(nullable: false),
                    Ok_Laser1 = table.Column<bool>(nullable: false),
                    Ok_Laser2 = table.Column<bool>(nullable: false),
                    Ok_Laser3 = table.Column<bool>(nullable: false),
                    Ok_Shape1 = table.Column<bool>(nullable: false),
                    Ok_Shape2 = table.Column<bool>(nullable: false),
                    Ok_Shape3 = table.Column<bool>(nullable: false),
                    Ok_Shutter1 = table.Column<bool>(nullable: false),
                    Ok_Shutter2 = table.Column<bool>(nullable: false),
                    Ok_Shutter3 = table.Column<bool>(nullable: false),
                    Origin_X_Axis = table.Column<bool>(nullable: false),
                    Origin_Y_Axis = table.Column<bool>(nullable: false),
                    Set_Y_Resolution = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InstallationInfos", x => x.InstallationInfoId);
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
                name: "InstallationInfos");

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
