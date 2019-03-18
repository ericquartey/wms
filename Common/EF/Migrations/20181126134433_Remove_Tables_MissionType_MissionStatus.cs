using Microsoft.EntityFrameworkCore.Migrations;

namespace Ferretto.Common.EF.Migrations
{
    [System.CodeDom.Compiler.GeneratedCode("EntityFramework", "v2.1")]
    public partial class Remove_Tables_MissionType_MissionStatus : Migration
    {
        #region Fields

        private const string MissionsTable = "Missions";

        #endregion

        #region Methods

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            if (migrationBuilder == null)
            {
                throw new System.ArgumentNullException(nameof(migrationBuilder));
            }

            migrationBuilder.DropColumn(
                name: "Status",
                table: MissionsTable);

            migrationBuilder.AddColumn<string>(
                name: "MissionStatusId",
                table: MissionsTable,
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
                table: MissionsTable,
                column: "MissionStatusId");

            migrationBuilder.AddForeignKey(
                name: "FK_Missions_MissionStatuses_MissionStatusId",
                table: MissionsTable,
                column: "MissionStatusId",
                principalTable: "MissionStatuses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            if (migrationBuilder == null)
            {
                throw new System.ArgumentNullException(nameof(migrationBuilder));
            }

            migrationBuilder.DropForeignKey(
                name: "FK_Missions_MissionStatuses_MissionStatusId",
                table: MissionsTable);

            migrationBuilder.DropTable(
                name: "MissionStatuses");

            migrationBuilder.DropIndex(
                name: "IX_Missions_MissionStatusId",
                table: MissionsTable);

            migrationBuilder.DropColumn(
                name: "MissionStatusId",
                table: MissionsTable);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: MissionsTable,
                type: "char(1)",
                nullable: false,
                defaultValueSql: "'N'");
        }

        #endregion
    }
}
