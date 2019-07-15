using Microsoft.EntityFrameworkCore.Migrations;

namespace Ferretto.Common.EF.Migrations
{
    #pragma warning disable

    public partial class Rename_Height_Field_In_CompartmentType_Table_To_Depth : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Height",
                table: "CompartmentTypes",
                newName: "Depth");

            migrationBuilder.AlterColumn<double>(
                name: "MinStepCompartment",
                table: "GlobalSettings",
                nullable: false,
                oldClrType: typeof(double),
                oldDefaultValue: 5.0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Depth",
                table: "CompartmentTypes",
                newName: "Height");

            migrationBuilder.AlterColumn<double>(
                name: "MinStepCompartment",
                table: "GlobalSettings",
                nullable: false,
                defaultValue: 5.0,
                oldClrType: typeof(double));
        }
    }

    #pragma warning restore
}
