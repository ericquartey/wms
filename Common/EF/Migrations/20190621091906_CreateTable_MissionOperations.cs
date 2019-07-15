using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Ferretto.Common.EF.Migrations
{
    [System.CodeDom.Compiler.GeneratedCode("EntityFramework", "v2.2")]
    public partial class CreateTable_MissionOperations : Migration
    {
        #region Methods

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MissionOperations");

            migrationBuilder.AlterColumn<int>(
                name: "LoadingUnitId",
                table: "Missions",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.AddColumn<int>(
                name: "CompartmentId",
                table: "Missions",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "DispatchedQuantity",
                table: "Missions",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<int>(
                name: "ItemId",
                table: "Missions",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ItemListId",
                table: "Missions",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ItemListRowId",
                table: "Missions",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Lot",
                table: "Missions",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MaterialStatusId",
                table: "Missions",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PackageTypeId",
                table: "Missions",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RegistrationNumber",
                table: "Missions",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "RequestedQuantity",
                table: "Missions",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<string>(
                name: "Sub1",
                table: "Missions",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Sub2",
                table: "Missions",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "Missions",
                type: "char(1)",
                nullable: false,
                defaultValue: string.Empty);

            migrationBuilder.AlterColumn<double>(
                name: "MinStepCompartment",
                table: "GlobalSettings",
                nullable: false,
                defaultValue: 5.0,
                oldClrType: typeof(double));

            migrationBuilder.CreateIndex(
                name: "IX_Missions_CompartmentId",
                table: "Missions",
                column: "CompartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Missions_ItemId",
                table: "Missions",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_Missions_ItemListId",
                table: "Missions",
                column: "ItemListId");

            migrationBuilder.CreateIndex(
                name: "IX_Missions_ItemListRowId",
                table: "Missions",
                column: "ItemListRowId");

            migrationBuilder.CreateIndex(
                name: "IX_Missions_MaterialStatusId",
                table: "Missions",
                column: "MaterialStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_Missions_PackageTypeId",
                table: "Missions",
                column: "PackageTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Missions_Compartments_CompartmentId",
                table: "Missions",
                column: "CompartmentId",
                principalTable: "Compartments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Missions_Items_ItemId",
                table: "Missions",
                column: "ItemId",
                principalTable: "Items",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Missions_ItemLists_ItemListId",
                table: "Missions",
                column: "ItemListId",
                principalTable: "ItemLists",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Missions_ItemListRows_ItemListRowId",
                table: "Missions",
                column: "ItemListRowId",
                principalTable: "ItemListRows",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Missions_MaterialStatuses_MaterialStatusId",
                table: "Missions",
                column: "MaterialStatusId",
                principalTable: "MaterialStatuses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Missions_PackageTypes_PackageTypeId",
                table: "Missions",
                column: "PackageTypeId",
                principalTable: "PackageTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Missions_Compartments_CompartmentId",
                table: "Missions");

            migrationBuilder.DropForeignKey(
                name: "FK_Missions_Items_ItemId",
                table: "Missions");

            migrationBuilder.DropForeignKey(
                name: "FK_Missions_ItemLists_ItemListId",
                table: "Missions");

            migrationBuilder.DropForeignKey(
                name: "FK_Missions_ItemListRows_ItemListRowId",
                table: "Missions");

            migrationBuilder.DropForeignKey(
                name: "FK_Missions_MaterialStatuses_MaterialStatusId",
                table: "Missions");

            migrationBuilder.DropForeignKey(
                name: "FK_Missions_PackageTypes_PackageTypeId",
                table: "Missions");

            migrationBuilder.DropIndex(
                name: "IX_Missions_CompartmentId",
                table: "Missions");

            migrationBuilder.DropIndex(
                name: "IX_Missions_ItemId",
                table: "Missions");

            migrationBuilder.DropIndex(
                name: "IX_Missions_ItemListId",
                table: "Missions");

            migrationBuilder.DropIndex(
                name: "IX_Missions_ItemListRowId",
                table: "Missions");

            migrationBuilder.DropIndex(
                name: "IX_Missions_MaterialStatusId",
                table: "Missions");

            migrationBuilder.DropIndex(
                name: "IX_Missions_PackageTypeId",
                table: "Missions");

            migrationBuilder.DropColumn(
                name: "CompartmentId",
                table: "Missions");

            migrationBuilder.DropColumn(
                name: "DispatchedQuantity",
                table: "Missions");

            migrationBuilder.DropColumn(
                name: "ItemId",
                table: "Missions");

            migrationBuilder.DropColumn(
                name: "ItemListId",
                table: "Missions");

            migrationBuilder.DropColumn(
                name: "ItemListRowId",
                table: "Missions");

            migrationBuilder.DropColumn(
                name: "Lot",
                table: "Missions");

            migrationBuilder.DropColumn(
                name: "MaterialStatusId",
                table: "Missions");

            migrationBuilder.DropColumn(
                name: "PackageTypeId",
                table: "Missions");

            migrationBuilder.DropColumn(
                name: "RegistrationNumber",
                table: "Missions");

            migrationBuilder.DropColumn(
                name: "RequestedQuantity",
                table: "Missions");

            migrationBuilder.DropColumn(
                name: "Sub1",
                table: "Missions");

            migrationBuilder.DropColumn(
                name: "Sub2",
                table: "Missions");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "Missions");

            migrationBuilder.AlterColumn<int>(
                name: "LoadingUnitId",
                table: "Missions",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.AlterColumn<double>(
                name: "MinStepCompartment",
                table: "GlobalSettings",
                nullable: false,
                oldClrType: typeof(double),
                oldDefaultValue: 5.0);

            migrationBuilder.CreateTable(
                name: "MissionOperations",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CompartmentId = table.Column<int>(nullable: false),
                    CreationDate = table.Column<DateTime>(nullable: false, defaultValueSql: "GETUTCDATE()"),
                    DispatchedQuantity = table.Column<double>(nullable: false),
                    ItemId = table.Column<int>(nullable: false),
                    ItemListId = table.Column<int>(nullable: true),
                    ItemListRowId = table.Column<int>(nullable: true),
                    LastModificationDate = table.Column<DateTime>(nullable: false, defaultValueSql: "GETUTCDATE()"),
                    Lot = table.Column<string>(nullable: true),
                    MaterialStatusId = table.Column<int>(nullable: true),
                    MissionId = table.Column<int>(nullable: false),
                    PackageTypeId = table.Column<int>(nullable: true),
                    Priority = table.Column<int>(nullable: false),
                    RegistrationNumber = table.Column<string>(nullable: true),
                    RequestedQuantity = table.Column<double>(nullable: false),
                    Status = table.Column<string>(type: "char(1)", nullable: false, defaultValueSql: "'N'"),
                    Sub1 = table.Column<string>(nullable: true),
                    Sub2 = table.Column<string>(nullable: true),
                    Type = table.Column<string>(type: "char(1)", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MissionOperations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MissionOperations_Compartments_CompartmentId",
                        column: x => x.CompartmentId,
                        principalTable: "Compartments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MissionOperations_Items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MissionOperations_ItemLists_ItemListId",
                        column: x => x.ItemListId,
                        principalTable: "ItemLists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MissionOperations_ItemListRows_ItemListRowId",
                        column: x => x.ItemListRowId,
                        principalTable: "ItemListRows",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MissionOperations_MaterialStatuses_MaterialStatusId",
                        column: x => x.MaterialStatusId,
                        principalTable: "MaterialStatuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MissionOperations_Missions_MissionId",
                        column: x => x.MissionId,
                        principalTable: "Missions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MissionOperations_PackageTypes_PackageTypeId",
                        column: x => x.PackageTypeId,
                        principalTable: "PackageTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MissionOperations_CompartmentId",
                table: "MissionOperations",
                column: "CompartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_MissionOperations_ItemId",
                table: "MissionOperations",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_MissionOperations_ItemListId",
                table: "MissionOperations",
                column: "ItemListId");

            migrationBuilder.CreateIndex(
                name: "IX_MissionOperations_ItemListRowId",
                table: "MissionOperations",
                column: "ItemListRowId");

            migrationBuilder.CreateIndex(
                name: "IX_MissionOperations_MaterialStatusId",
                table: "MissionOperations",
                column: "MaterialStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_MissionOperations_MissionId",
                table: "MissionOperations",
                column: "MissionId");

            migrationBuilder.CreateIndex(
                name: "IX_MissionOperations_PackageTypeId",
                table: "MissionOperations",
                column: "PackageTypeId");
        }

        #endregion
    }
}
