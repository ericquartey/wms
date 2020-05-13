using Microsoft.EntityFrameworkCore.Migrations;

namespace Ferretto.VW.MAS.DataLayer.Migrations
{
    public partial class change_value_type_on_InverterParameter : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Value",
                table: "InverterParameter",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.InsertData(
                table: "ErrorStatistics",
                columns: new[] { "Code", "TotalErrors" },
                values: new object[] { 77, 0 });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "ErrorStatistics",
                keyColumn: "Code",
                keyValue: 77);

            migrationBuilder.AlterColumn<int>(
                name: "Value",
                table: "InverterParameter",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);
        }
    }
}
