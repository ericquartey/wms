using Microsoft.EntityFrameworkCore.Migrations;

namespace Ferretto.Common.EF.Migrations
{
    #pragma warning disable

    public partial class Rename_Length_Field_To_Depth : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Length",
                table: "LoadingUnitSizeClasses",
                newName: "Depth");

            migrationBuilder.RenameColumn(
                name: "Length",
                table: "Items",
                newName: "Depth");

            migrationBuilder.RenameColumn(
                name: "Length",
                table: "CellSizeClasses",
                newName: "Depth");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Depth",
                table: "LoadingUnitSizeClasses",
                newName: "Length");

            migrationBuilder.RenameColumn(
                name: "Depth",
                table: "Items",
                newName: "Length");

            migrationBuilder.RenameColumn(
                name: "Depth",
                table: "CellSizeClasses",
                newName: "Length");
        }
    }

    #pragma warning restore
}
