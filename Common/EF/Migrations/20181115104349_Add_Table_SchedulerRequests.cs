using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Ferretto.Common.EF.Migrations
{
    public partial class Add_Table_SchedulerRequests : Migration
    {
        #region Fields

        private const string SchedulerRequestsName = "SchedulerRequests";

        #endregion

        #region Methods

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            if (migrationBuilder == null)
            {
                throw new System.ArgumentNullException(nameof(migrationBuilder));
            }

            migrationBuilder.DropTable(
                name: SchedulerRequestsName);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreationDate",
                table: "LoadingUnits",
                nullable: false,
                defaultValueSql: "GETDATE()",
                oldClrType: typeof(DateTime),
                oldDefaultValueSql: "GETUTCDATE()");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreationDate",
                table: "Items",
                nullable: false,
                defaultValueSql: "GETDATE()",
                oldClrType: typeof(DateTime),
                oldDefaultValueSql: "GETUTCDATE()");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreationDate",
                table: "ItemLists",
                nullable: false,
                defaultValueSql: "GETDATE()",
                oldClrType: typeof(DateTime),
                oldDefaultValueSql: "GETUTCDATE()");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreationDate",
                table: "ItemListRows",
                nullable: false,
                defaultValueSql: "GETDATE()",
                oldClrType: typeof(DateTime),
                oldDefaultValueSql: "GETUTCDATE()");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreationDate",
                table: "Compartments",
                nullable: false,
                defaultValueSql: "GETDATE()",
                oldClrType: typeof(DateTime),
                oldDefaultValueSql: "GETUTCDATE()");
        }

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            if (migrationBuilder == null)
            {
                throw new System.ArgumentNullException(nameof(migrationBuilder));
            }

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreationDate",
                table: "LoadingUnits",
                nullable: false,
                defaultValueSql: "GETUTCDATE()",
                oldClrType: typeof(DateTime),
                oldDefaultValueSql: "GETDATE()");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreationDate",
                table: "Items",
                nullable: false,
                defaultValueSql: "GETUTCDATE()",
                oldClrType: typeof(DateTime),
                oldDefaultValueSql: "GETDATE()");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreationDate",
                table: "ItemLists",
                nullable: false,
                defaultValueSql: "GETUTCDATE()",
                oldClrType: typeof(DateTime),
                oldDefaultValueSql: "GETDATE()");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreationDate",
                table: "ItemListRows",
                nullable: false,
                defaultValueSql: "GETUTCDATE()",
                oldClrType: typeof(DateTime),
                oldDefaultValueSql: "GETDATE()");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreationDate",
                table: "Compartments",
                nullable: false,
                defaultValueSql: "GETUTCDATE()",
                oldClrType: typeof(DateTime),
                oldDefaultValueSql: "GETDATE()");

            migrationBuilder.CreateTable(
                name: "SchedulerRequests",
                columns: table => new
                {
                    AreaId = table.Column<int>(nullable: false),
                    BayId = table.Column<int>(nullable: true),
                    CreationDate = table.Column<DateTime>(nullable: false, defaultValueSql: "GETUTCDATE()"),
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    IsInstant = table.Column<bool>(nullable: false),
                    ItemId = table.Column<int>(nullable: true),
                    ListId = table.Column<int>(nullable: true),
                    ListRowId = table.Column<int>(nullable: true),
                    LoadingUnitId = table.Column<int>(nullable: true),
                    LoadingUnitTypeId = table.Column<int>(nullable: true),
                    Lot = table.Column<string>(nullable: true),
                    MaterialStatusId = table.Column<int>(nullable: true),
                    OperationType = table.Column<string>(type: "char(1)", nullable: false),
                    PackageTypeId = table.Column<int>(nullable: true),
                    RegistrationNumber = table.Column<string>(nullable: true),
                    RequestedQuantity = table.Column<int>(nullable: true),
                    Sub1 = table.Column<string>(nullable: true),
                    Sub2 = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SchedulerRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SchedulerRequests_Areas_AreaId",
                        column: x => x.AreaId,
                        principalTable: "Areas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SchedulerRequests_Bays_BayId",
                        column: x => x.BayId,
                        principalTable: "Bays",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SchedulerRequests_Items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SchedulerRequests_ItemLists_ListId",
                        column: x => x.ListId,
                        principalTable: "ItemLists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SchedulerRequests_ItemListRows_ListRowId",
                        column: x => x.ListRowId,
                        principalTable: "ItemListRows",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SchedulerRequests_LoadingUnits_LoadingUnitId",
                        column: x => x.LoadingUnitId,
                        principalTable: "LoadingUnits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SchedulerRequests_LoadingUnitTypes_LoadingUnitTypeId",
                        column: x => x.LoadingUnitTypeId,
                        principalTable: "LoadingUnitTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SchedulerRequests_MaterialStatuses_MaterialStatusId",
                        column: x => x.MaterialStatusId,
                        principalTable: "MaterialStatuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SchedulerRequests_PackageTypes_PackageTypeId",
                        column: x => x.PackageTypeId,
                        principalTable: "PackageTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SchedulerRequests_AreaId",
                table: "SchedulerRequests",
                column: "AreaId");

            migrationBuilder.CreateIndex(
                name: "IX_SchedulerRequests_BayId",
                table: "SchedulerRequests",
                column: "BayId");

            migrationBuilder.CreateIndex(
                name: "IX_SchedulerRequests_ItemId",
                table: "SchedulerRequests",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_SchedulerRequests_ListId",
                table: "SchedulerRequests",
                column: "ListId");

            migrationBuilder.CreateIndex(
                name: "IX_SchedulerRequests_ListRowId",
                table: "SchedulerRequests",
                column: "ListRowId");

            migrationBuilder.CreateIndex(
                name: "IX_SchedulerRequests_LoadingUnitId",
                table: "SchedulerRequests",
                column: "LoadingUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_SchedulerRequests_LoadingUnitTypeId",
                table: "SchedulerRequests",
                column: "LoadingUnitTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_SchedulerRequests_MaterialStatusId",
                table: "SchedulerRequests",
                column: "MaterialStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_SchedulerRequests_PackageTypeId",
                table: "SchedulerRequests",
                column: "PackageTypeId");
        }

        #endregion
    }
}
