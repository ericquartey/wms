using Microsoft.EntityFrameworkCore.Migrations;

namespace Ferretto.VW.MAS.DataLayer.Migrations
{
    public partial class WMS_Settings_Defaults : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "WmsSettings",
                keyColumn: "Id",
                keyValue: -1,
                column: "ServiceUrl",
                value: "http://127.0.0.1:10000/");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "WmsSettings",
                keyColumn: "Id",
                keyValue: -1,
                column: "ServiceUrl",
                value: null);
        }
    }
}
