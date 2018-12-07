using Microsoft.EntityFrameworkCore.Migrations;

namespace Ferretto.Common.EF.Migrations
{
    public partial class Rename_Priority_And_Status_Fields_In_Tables_ItemList_ItemListRow : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ItemListStatus",
                table: "ItemLists");

            migrationBuilder.DropColumn(
                name: "ItemListRowStatus",
                table: "ItemListRows");

            migrationBuilder.RenameColumn(
                name: "RowPriority",
                table: "ItemListRows",
                newName: "Priority");

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "ItemLists",
                type: "char(1)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "ItemListRows",
                type: "char(1)",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "ItemLists");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "ItemListRows");

            migrationBuilder.RenameColumn(
                name: "Priority",
                table: "ItemListRows",
                newName: "RowPriority");

            migrationBuilder.AddColumn<string>(
                name: "ItemListStatus",
                table: "ItemLists",
                type: "char(1)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ItemListRowStatus",
                table: "ItemListRows",
                type: "char(1)",
                nullable: false,
                defaultValue: "");
        }
    }
}
