using Microsoft.EntityFrameworkCore.Migrations;

namespace Ferretto.Common.EF.Migrations
{
    [System.CodeDom.Compiler.GeneratedCode("EntityFramework", "v2.1")]
    public partial class Rename_EvadedQuantity_Field_In_ItemListRows_ToDispatchedQuantity : Migration
    {
        #region Fields

        private const string ItemListRowsTable = "ItemListRows";

        #endregion

        #region Methods

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            if (migrationBuilder == null)
            {
                throw new System.ArgumentNullException(nameof(migrationBuilder));
            }

            migrationBuilder.RenameColumn(
                name: "DispatchedQuantity",
                table: ItemListRowsTable,
                newName: "EvadedQuantity");

            migrationBuilder.AlterColumn<int>(
                name: "PackageTypeId",
                table: ItemListRowsTable,
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "MaterialStatusId",
                table: ItemListRowsTable,
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);
        }

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            if (migrationBuilder == null)
            {
                throw new System.ArgumentNullException(nameof(migrationBuilder));
            }

            migrationBuilder.RenameColumn(
                name: "EvadedQuantity",
                table: ItemListRowsTable,
                newName: "DispatchedQuantity");

            migrationBuilder.AlterColumn<int>(
                name: "PackageTypeId",
                table: ItemListRowsTable,
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.AlterColumn<int>(
                name: "MaterialStatusId",
                table: ItemListRowsTable,
                nullable: true,
                oldClrType: typeof(int));
        }

        #endregion
    }
}
