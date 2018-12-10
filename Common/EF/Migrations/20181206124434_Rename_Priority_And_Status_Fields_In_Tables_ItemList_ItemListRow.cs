using Microsoft.EntityFrameworkCore.Migrations;

namespace Ferretto.Common.EF.Migrations
{
    public partial class Rename_Priority_And_Status_Fields_In_Tables_ItemList_ItemListRow : Migration
    {
        #region Methods

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Status",
                table: "ItemListRows",
                newName: "ItemListRowStatus");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "ItemLists",
                newName: "ItemListStatus");

            migrationBuilder.RenameColumn(
                name: "Priority",
                table: "ItemListRows",
                newName: "RowPriority");
        }

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ItemListRowStatus",
                table: "ItemListRows",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "ItemListStatus",
                table: "ItemLists",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "RowPriority",
                table: "ItemListRows",
                newName: "Priority");
        }

        #endregion Methods
    }
}
