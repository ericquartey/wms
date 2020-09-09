using Microsoft.EntityFrameworkCore.Migrations;

namespace Ferretto.VW.MAS.DataLayer.Migrations
{
    public partial class SocketLinkParams : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "SocketLinkIsEnabled",
                table: "WmsSettings",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "SocketLinkPolling",
                table: "WmsSettings",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SocketLinkPort",
                table: "WmsSettings",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SocketLinkTimeout",
                table: "WmsSettings",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "WmsSettings",
                keyColumn: "Id",
                keyValue: -1,
                columns: new[] { "SocketLinkPolling", "SocketLinkPort", "SocketLinkTimeout" },
                values: new object[] { 0, 7075, 600 });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SocketLinkIsEnabled",
                table: "WmsSettings");

            migrationBuilder.DropColumn(
                name: "SocketLinkPolling",
                table: "WmsSettings");

            migrationBuilder.DropColumn(
                name: "SocketLinkPort",
                table: "WmsSettings");

            migrationBuilder.DropColumn(
                name: "SocketLinkTimeout",
                table: "WmsSettings");
        }
    }
}
