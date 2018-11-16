using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Ferretto.Common.EF.Migrations
{
    public partial class Delete_Table_ItemManagementType : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Items_ItemManagementTypes_ItemManagementTypeId",
                table: "Items");

            migrationBuilder.DropTable(
                name: "ItemManagementTypes");

            migrationBuilder.DropIndex(
                name: "IX_Items_ItemManagementTypeId",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "ItemManagementTypeId",
                table: "Items");

            migrationBuilder.AlterColumn<int>(
                name: "RequestedQuantity",
                table: "SchedulerRequests",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "ItemId",
                table: "SchedulerRequests",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ManagementType",
                table: "Items",
                type: "char(1)",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ManagementType",
                table: "Items");

            migrationBuilder.AlterColumn<int>(
                name: "RequestedQuantity",
                table: "SchedulerRequests",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.AlterColumn<int>(
                name: "ItemId",
                table: "SchedulerRequests",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.AddColumn<int>(
                name: "ItemManagementTypeId",
                table: "Items",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ItemManagementTypes",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Description = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemManagementTypes", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Items_ItemManagementTypeId",
                table: "Items",
                column: "ItemManagementTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Items_ItemManagementTypes_ItemManagementTypeId",
                table: "Items",
                column: "ItemManagementTypeId",
                principalTable: "ItemManagementTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
