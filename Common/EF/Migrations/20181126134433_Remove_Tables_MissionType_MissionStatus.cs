using Microsoft.EntityFrameworkCore.Migrations;

namespace Ferretto.Common.EF.Migrations
{
    public partial class Remove_Tables_MissionType_MissionStatus : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Missions_MissionStatuses_MissionStatusId",
                table: "Missions");

            migrationBuilder.DropTable(
                name: "MissionStatuses");

            migrationBuilder.DropIndex(
                name: "IX_Missions_MissionStatusId",
                table: "Missions");

            migrationBuilder.DropColumn(
                name: "MissionStatusId",
                table: "Missions");

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Missions",
                type: "char(1)",
                nullable: false,
                defaultValueSql: "N");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "Missions");

            migrationBuilder.AddColumn<string>(
                name: "MissionStatusId",
                table: "Missions",
                type: "char(1)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "MissionStatuses",
                columns: table => new
                {
                    Id = table.Column<string>(type: "char(1)", nullable: false),
                    Description = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MissionStatuses", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Missions_MissionStatusId",
                table: "Missions",
                column: "MissionStatusId");

            migrationBuilder.AddForeignKey(
                name: "FK_Missions_MissionStatuses_MissionStatusId",
                table: "Missions",
                column: "MissionStatusId",
                principalTable: "MissionStatuses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
