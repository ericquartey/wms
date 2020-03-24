using Microsoft.EntityFrameworkCore.Migrations;

namespace Ferretto.VW.MAS.DataLayer.Migrations
{
    public partial class WmsSettingsUrl : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IsWmsTimeSyncEnabled",
                table: "WmsSettings",
                newName: "IsTimeSyncEnabled");

            migrationBuilder.AddColumn<bool>(
                name: "IsEnabled",
                table: "WmsSettings",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ServiceUrl",
                table: "WmsSettings",
                type: "text",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "WmsSettings",
                keyColumn: "Id",
                keyValue: -1,
                column: "IsTimeSyncEnabled",
                value: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsEnabled",
                table: "WmsSettings");

            migrationBuilder.DropColumn(
                name: "ServiceUrl",
                table: "WmsSettings");

            migrationBuilder.RenameColumn(
                name: "IsTimeSyncEnabled",
                table: "WmsSettings",
                newName: "IsWmsTimeSyncEnabled");

            migrationBuilder.UpdateData(
                table: "WmsSettings",
                keyColumn: "Id",
                keyValue: -1,
                column: "IsWmsTimeSyncEnabled",
                value: true);
        }
    }
}
