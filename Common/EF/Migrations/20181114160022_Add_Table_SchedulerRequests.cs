using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Ferretto.Common.EF.Migrations
{
    public partial class Add_Table_SchedulerRequests : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SchedulerRequest",
                columns: table => new
                {
                    AreaId = table.Column<int>(nullable: false),
                    BayId = table.Column<int>(nullable: true),
                    CreationDate = table.Column<DateTime>(nullable: false),
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
                    OperationType = table.Column<int>(nullable: false),
                    PackageTypeId = table.Column<int>(nullable: false),
                    RegistrationNumber = table.Column<string>(nullable: true),
                    RequestedQuantity = table.Column<int>(nullable: true),
                    Sub1 = table.Column<string>(nullable: true),
                    Sub2 = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SchedulerRequest", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SchedulerRequest_Areas_AreaId",
                        column: x => x.AreaId,
                        principalTable: "Areas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SchedulerRequest_Bays_BayId",
                        column: x => x.BayId,
                        principalTable: "Bays",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SchedulerRequest_Items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SchedulerRequest_ItemLists_ListId",
                        column: x => x.ListId,
                        principalTable: "ItemLists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SchedulerRequest_ItemListRows_ListRowId",
                        column: x => x.ListRowId,
                        principalTable: "ItemListRows",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SchedulerRequest_LoadingUnits_LoadingUnitId",
                        column: x => x.LoadingUnitId,
                        principalTable: "LoadingUnits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SchedulerRequest_LoadingUnitTypes_LoadingUnitTypeId",
                        column: x => x.LoadingUnitTypeId,
                        principalTable: "LoadingUnitTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SchedulerRequest_MaterialStatuses_MaterialStatusId",
                        column: x => x.MaterialStatusId,
                        principalTable: "MaterialStatuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SchedulerRequest_PackageTypes_PackageTypeId",
                        column: x => x.PackageTypeId,
                        principalTable: "PackageTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SchedulerRequest_AreaId",
                table: "SchedulerRequest",
                column: "AreaId");

            migrationBuilder.CreateIndex(
                name: "IX_SchedulerRequest_BayId",
                table: "SchedulerRequest",
                column: "BayId");

            migrationBuilder.CreateIndex(
                name: "IX_SchedulerRequest_ItemId",
                table: "SchedulerRequest",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_SchedulerRequest_ListId",
                table: "SchedulerRequest",
                column: "ListId");

            migrationBuilder.CreateIndex(
                name: "IX_SchedulerRequest_ListRowId",
                table: "SchedulerRequest",
                column: "ListRowId");

            migrationBuilder.CreateIndex(
                name: "IX_SchedulerRequest_LoadingUnitId",
                table: "SchedulerRequest",
                column: "LoadingUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_SchedulerRequest_LoadingUnitTypeId",
                table: "SchedulerRequest",
                column: "LoadingUnitTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_SchedulerRequest_MaterialStatusId",
                table: "SchedulerRequest",
                column: "MaterialStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_SchedulerRequest_PackageTypeId",
                table: "SchedulerRequest",
                column: "PackageTypeId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SchedulerRequest");
        }
    }
}
