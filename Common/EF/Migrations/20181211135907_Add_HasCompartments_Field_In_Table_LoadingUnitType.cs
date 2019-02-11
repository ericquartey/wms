using Microsoft.EntityFrameworkCore.Migrations;

namespace Ferretto.Common.EF.Migrations
{
    public partial class Add_HasCompartments_Field_In_Table_LoadingUnitType : Migration
    {
        #region Methods

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            if (migrationBuilder == null)
            {
                throw new System.ArgumentNullException(nameof(migrationBuilder));
            }

            migrationBuilder.DropColumn(
                name: "HasCompartments",
                table: "LoadingUnitTypes");

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

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            if (migrationBuilder == null)
            {
                throw new System.ArgumentNullException(nameof(migrationBuilder));
            }

            migrationBuilder.AddColumn<bool>(
                name: "HasCompartments",
                table: "LoadingUnitTypes",
                nullable: false,
                defaultValue: false);

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

        #endregion
    }
}
