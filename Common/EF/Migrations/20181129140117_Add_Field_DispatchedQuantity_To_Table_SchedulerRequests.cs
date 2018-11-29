using Microsoft.EntityFrameworkCore.Migrations;

namespace Ferretto.Common.EF.Migrations
{
    public partial class Add_Field_DispatchedQuantity_To_Table_SchedulerRequests : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Missions_Bays_DestinationBayId",
                table: "Missions");

            migrationBuilder.DropForeignKey(
                name: "FK_Missions_Cells_DestinationCellId",
                table: "Missions");

            migrationBuilder.DropForeignKey(
                name: "FK_Missions_MissionTypes_MissionTypeId",
                table: "Missions");

            migrationBuilder.DropForeignKey(
                name: "FK_Missions_Bays_SourceBayId",
                table: "Missions");

            migrationBuilder.DropForeignKey(
                name: "FK_Missions_Cells_SourceCellId",
                table: "Missions");

            migrationBuilder.DropTable(
                name: "MissionTypes");

            migrationBuilder.DropIndex(
                name: "IX_Missions_DestinationBayId",
                table: "Missions");

            migrationBuilder.DropIndex(
                name: "IX_Missions_DestinationCellId",
                table: "Missions");

            migrationBuilder.DropIndex(
                name: "IX_Missions_MissionTypeId",
                table: "Missions");

            migrationBuilder.DropColumn(
                name: "DestinationBayId",
                table: "Missions");

            migrationBuilder.DropColumn(
                name: "DestinationCellId",
                table: "Missions");

            migrationBuilder.DropColumn(
                name: "MissionTypeId",
                table: "Missions");

            migrationBuilder.RenameColumn(
                name: "SourceCellId",
                table: "Missions",
                newName: "CellId");

            migrationBuilder.RenameColumn(
                name: "SourceBayId",
                table: "Missions",
                newName: "BayId");

            migrationBuilder.RenameIndex(
                name: "IX_Missions_SourceCellId",
                table: "Missions",
                newName: "IX_Missions_CellId");

            migrationBuilder.RenameIndex(
                name: "IX_Missions_SourceBayId",
                table: "Missions",
                newName: "IX_Missions_BayId");

            migrationBuilder.AddColumn<int>(
                name: "DispatchedQuantity",
                table: "SchedulerRequests",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "Missions",
                type: "char(1)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddForeignKey(
                name: "FK_Missions_Bays_BayId",
                table: "Missions",
                column: "BayId",
                principalTable: "Bays",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Missions_Cells_CellId",
                table: "Missions",
                column: "CellId",
                principalTable: "Cells",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Missions_Bays_BayId",
                table: "Missions");

            migrationBuilder.DropForeignKey(
                name: "FK_Missions_Cells_CellId",
                table: "Missions");

            migrationBuilder.DropColumn(
                name: "DispatchedQuantity",
                table: "SchedulerRequests");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "Missions");

            migrationBuilder.RenameColumn(
                name: "CellId",
                table: "Missions",
                newName: "SourceCellId");

            migrationBuilder.RenameColumn(
                name: "BayId",
                table: "Missions",
                newName: "SourceBayId");

            migrationBuilder.RenameIndex(
                name: "IX_Missions_CellId",
                table: "Missions",
                newName: "IX_Missions_SourceCellId");

            migrationBuilder.RenameIndex(
                name: "IX_Missions_BayId",
                table: "Missions",
                newName: "IX_Missions_SourceBayId");

            migrationBuilder.AddColumn<int>(
                name: "DestinationBayId",
                table: "Missions",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DestinationCellId",
                table: "Missions",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MissionTypeId",
                table: "Missions",
                type: "char(2)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "MissionTypes",
                columns: table => new
                {
                    Id = table.Column<string>(type: "char(2)", nullable: false),
                    Description = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MissionTypes", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Missions_DestinationBayId",
                table: "Missions",
                column: "DestinationBayId");

            migrationBuilder.CreateIndex(
                name: "IX_Missions_DestinationCellId",
                table: "Missions",
                column: "DestinationCellId");

            migrationBuilder.CreateIndex(
                name: "IX_Missions_MissionTypeId",
                table: "Missions",
                column: "MissionTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Missions_Bays_DestinationBayId",
                table: "Missions",
                column: "DestinationBayId",
                principalTable: "Bays",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Missions_Cells_DestinationCellId",
                table: "Missions",
                column: "DestinationCellId",
                principalTable: "Cells",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Missions_MissionTypes_MissionTypeId",
                table: "Missions",
                column: "MissionTypeId",
                principalTable: "MissionTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Missions_Bays_SourceBayId",
                table: "Missions",
                column: "SourceBayId",
                principalTable: "Bays",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Missions_Cells_SourceCellId",
                table: "Missions",
                column: "SourceCellId",
                principalTable: "Cells",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
