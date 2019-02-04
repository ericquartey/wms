using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Ferretto.Common.EF.Migrations
{
    public partial class Delete_Table_ItemManagementType : Migration
    {
        #region Fields

        private const string ItemsTable = "Items";

        private const string SchedulerRequestsTable = "SchedulerRequests";

        #endregion

        #region Methods

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            if (migrationBuilder == null)
            {
                throw new System.ArgumentNullException(nameof(migrationBuilder));
            }

            migrationBuilder.DropColumn(
                name: "ManagementType",
                table: ItemsTable);

            migrationBuilder.AlterColumn<int>(
                name: "RequestedQuantity",
                table: SchedulerRequestsTable,
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.AlterColumn<int>(
                name: "ItemId",
                table: "SchedulerRequests",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.AddColumn<int>(
                name: "ItemManagementTypeId",
                table: ItemsTable,
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
                table: ItemsTable,
                column: "ItemManagementTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Items_ItemManagementTypes_ItemManagementTypeId",
                table: ItemsTable,
                column: "ItemManagementTypeId",
                principalTable: "ItemManagementTypes",
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
                name: "FK_Items_ItemManagementTypes_ItemManagementTypeId",
                table: ItemsTable);

            migrationBuilder.DropTable(
                name: "ItemManagementTypes");

            migrationBuilder.DropIndex(
                name: "IX_Items_ItemManagementTypeId",
                table: ItemsTable);

            migrationBuilder.DropColumn(
                name: "ItemManagementTypeId",
                table: ItemsTable);

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
                table: ItemsTable,
                type: "char(1)",
                nullable: false,
                defaultValue: string.Empty);
        }

        #endregion
    }
}
