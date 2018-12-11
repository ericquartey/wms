using Microsoft.EntityFrameworkCore.Migrations;

namespace Ferretto.Common.EF.Migrations
{
    public partial class Rename_EvadedQuantity_Field_In_ItemListRows_ToDispatchedQuantity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "EvadedQuantity",
                table: "ItemListRows",
                newName: "DispatchedQuantity");

            migrationBuilder.AlterColumn<int>(
                name: "PackageTypeId",
                table: "ItemListRows",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.AlterColumn<int>(
                name: "MaterialStatusId",
                table: "ItemListRows",
                nullable: true,
                oldClrType: typeof(int));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DispatchedQuantity",
                table: "ItemListRows",
                newName: "EvadedQuantity");

            migrationBuilder.AlterColumn<int>(
                name: "PackageTypeId",
                table: "ItemListRows",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "MaterialStatusId",
                table: "ItemListRows",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);
        }
    }
}
