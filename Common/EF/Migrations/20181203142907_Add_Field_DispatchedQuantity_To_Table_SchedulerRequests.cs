using Microsoft.EntityFrameworkCore.Migrations;

namespace Ferretto.Common.EF.Migrations
{
    public partial class Add_Field_DispatchedQuantity_To_Table_SchedulerRequests : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Missions_Bays_BayId1",
                table: "Missions");

            migrationBuilder.DropIndex(
                name: "IX_Missions_BayId1",
                table: "Missions");

            migrationBuilder.DropColumn(
                name: "BayId1",
                table: "Missions");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Missions",
                type: "char(1)",
                nullable: false,
                defaultValueSql: "'N'",
                oldClrType: typeof(string),
                oldType: "char(1)",
                oldDefaultValueSql: "N");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Missions",
                type: "char(1)",
                nullable: false,
                defaultValueSql: "N",
                oldClrType: typeof(string),
                oldType: "char(1)",
                oldDefaultValueSql: "'N'");

            migrationBuilder.AddColumn<int>(
                name: "BayId1",
                table: "Missions",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Missions_BayId1",
                table: "Missions",
                column: "BayId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Missions_Bays_BayId1",
                table: "Missions",
                column: "BayId1",
                principalTable: "Bays",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
