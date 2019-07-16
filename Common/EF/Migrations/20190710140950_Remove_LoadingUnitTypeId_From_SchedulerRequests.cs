using Microsoft.EntityFrameworkCore.Migrations;

namespace Ferretto.Common.EF.Migrations
{
    [System.CodeDom.Compiler.GeneratedCode("EntityFramework", "v2.2")]
    public partial class Remove_LoadingUnitTypeId_From_SchedulerRequests : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SchedulerRequests_LoadingUnitTypes_LoadingUnitTypeId",
                table: "SchedulerRequests");

            migrationBuilder.DropIndex(
                name: "IX_SchedulerRequests_LoadingUnitTypeId",
                table: "SchedulerRequests");

            migrationBuilder.DropColumn(
                name: "LoadingUnitTypeId",
                table: "SchedulerRequests");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "LoadingUnitTypeId",
                table: "SchedulerRequests",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SchedulerRequests_LoadingUnitTypeId",
                table: "SchedulerRequests",
                column: "LoadingUnitTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_SchedulerRequests_LoadingUnitTypes_LoadingUnitTypeId",
                table: "SchedulerRequests",
                column: "LoadingUnitTypeId",
                principalTable: "LoadingUnitTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
