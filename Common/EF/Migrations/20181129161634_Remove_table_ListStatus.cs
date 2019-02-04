using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Ferretto.Common.EF.Migrations
{
    public partial class Remove_table_ListStatus : Migration
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

            migrationBuilder.DropForeignKey(
                name: "FK_Missions_Bays_BayId",
                table: MissionsTable);

            migrationBuilder.DropForeignKey(
                name: "FK_Missions_Bays_BayId1",
                table: MissionsTable);

            migrationBuilder.DropForeignKey(
                name: "FK_Missions_Cells_CellId",
                table: MissionsTable);

            migrationBuilder.DropColumn(
                name: "DispatchedQuantity",
                table: "SchedulerRequests");

            migrationBuilder.DropColumn(
                name: "Type",
                table: MissionsTable);

            migrationBuilder.DropColumn(
                name: "ItemListStatus",
                table: "ItemLists");

            migrationBuilder.RenameColumn(
                name: "CellId",
                table: MissionsTable,
                newName: "SourceCellId");

            migrationBuilder.RenameColumn(
                name: "BayId1",
                table: MissionsTable,
                newName: "SourceBayId");

            migrationBuilder.RenameColumn(
                name: "BayId",
                table: MissionsTable,
                newName: "DestinationCellId");

            migrationBuilder.RenameIndex(
                name: "IX_Missions_CellId",
                table: MissionsTable,
                newName: "IX_Missions_SourceCellId");

            migrationBuilder.RenameIndex(
                name: "IX_Missions_BayId1",
                table: MissionsTable,
                newName: "IX_Missions_SourceBayId");

            migrationBuilder.RenameIndex(
                name: "IX_Missions_BayId",
                table: MissionsTable,
                newName: "IX_Missions_DestinationCellId");

            migrationBuilder.AddColumn<int>(
                name: "DestinationBayId",
                table: MissionsTable,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MissionTypeId",
                table: MissionsTable,
                type: "char(2)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ItemListStatusId",
                table: "ItemLists",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "ItemListStatuses",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Description = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemListStatuses", x => x.Id);
                });

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
                table: MissionsTable,
                column: "DestinationBayId");

            migrationBuilder.CreateIndex(
                name: "IX_Missions_MissionTypeId",
                table: MissionsTable,
                column: "MissionTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemLists_ItemListStatusId",
                table: "ItemLists",
                column: "ItemListStatusId");

            migrationBuilder.AddForeignKey(
                name: "FK_ItemLists_ItemListStatuses_ItemListStatusId",
                table: "ItemLists",
                column: "ItemListStatusId",
                principalTable: "ItemListStatuses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Missions_Bays_DestinationBayId",
                table: MissionsTable,
                column: "DestinationBayId",
                principalTable: "Bays",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Missions_Cells_DestinationCellId",
                table: MissionsTable,
                column: "DestinationCellId",
                principalTable: "Cells",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Missions_MissionTypes_MissionTypeId",
                table: MissionsTable,
                column: "MissionTypeId",
                principalTable: "MissionTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Missions_Bays_SourceBayId",
                table: MissionsTable,
                column: "SourceBayId",
                principalTable: "Bays",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Missions_Cells_SourceCellId",
                table: MissionsTable,
                column: "SourceCellId",
                principalTable: "Cells",
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
                name: "FK_ItemLists_ItemListStatuses_ItemListStatusId",
                table: "ItemLists");

            migrationBuilder.DropForeignKey(
                name: "FK_Missions_Bays_DestinationBayId",
                table: MissionsTable);

            migrationBuilder.DropForeignKey(
                name: "FK_Missions_Cells_DestinationCellId",
                table: MissionsTable);

            migrationBuilder.DropForeignKey(
                name: "FK_Missions_MissionTypes_MissionTypeId",
                table: MissionsTable);

            migrationBuilder.DropForeignKey(
                name: "FK_Missions_Bays_SourceBayId",
                table: MissionsTable);

            migrationBuilder.DropForeignKey(
                name: "FK_Missions_Cells_SourceCellId",
                table: MissionsTable);

            migrationBuilder.DropTable(
                name: "ItemListStatuses");

            migrationBuilder.DropTable(
                name: "MissionTypes");

            migrationBuilder.DropIndex(
                name: "IX_Missions_DestinationBayId",
                table: MissionsTable);

            migrationBuilder.DropIndex(
                name: "IX_Missions_MissionTypeId",
                table: MissionsTable);

            migrationBuilder.DropIndex(
                name: "IX_ItemLists_ItemListStatusId",
                table: "ItemLists");

            migrationBuilder.DropColumn(
                name: "DestinationBayId",
                table: MissionsTable);

            migrationBuilder.DropColumn(
                name: "MissionTypeId",
                table: MissionsTable);

            migrationBuilder.DropColumn(
                name: "ItemListStatusId",
                table: "ItemLists");

            migrationBuilder.RenameColumn(
                name: "SourceCellId",
                table: MissionsTable,
                newName: "CellId");

            migrationBuilder.RenameColumn(
                name: "SourceBayId",
                table: MissionsTable,
                newName: "BayId1");

            migrationBuilder.RenameColumn(
                name: "DestinationCellId",
                table: MissionsTable,
                newName: "BayId");

            migrationBuilder.RenameIndex(
                name: "IX_Missions_SourceCellId",
                table: MissionsTable,
                newName: "IX_Missions_CellId");

            migrationBuilder.RenameIndex(
                name: "IX_Missions_SourceBayId",
                table: MissionsTable,
                newName: "IX_Missions_BayId1");

            migrationBuilder.RenameIndex(
                name: "IX_Missions_DestinationCellId",
                table: MissionsTable,
                newName: "IX_Missions_BayId");

            migrationBuilder.AddColumn<int>(
                name: "DispatchedQuantity",
                table: "SchedulerRequests",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: MissionsTable,
                type: "char(1)",
                nullable: false,
                defaultValue: string.Empty);

            migrationBuilder.AddColumn<string>(
                name: "ItemListStatus",
                table: "ItemLists",
                type: "char(1)",
                nullable: false,
                defaultValue: string.Empty);

            migrationBuilder.AddForeignKey(
                name: "FK_Missions_Bays_BayId",
                table: MissionsTable,
                column: "BayId",
                principalTable: "Bays",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Missions_Bays_BayId1",
                table: MissionsTable,
                column: "BayId1",
                principalTable: "Bays",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Missions_Cells_CellId",
                table: MissionsTable,
                column: "CellId",
                principalTable: "Cells",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        #endregion
    }
}
