using Microsoft.EntityFrameworkCore.Migrations;

namespace Ferretto.Common.EF.Migrations
{
    [System.CodeDom.Compiler.GeneratedCode("EntityFramework", "v2.2")]
    public partial class Rename_Reference_Field_In_Table_LoadingUnits_To_ReferenceType : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Reference",
                table: "LoadingUnits");

            migrationBuilder.AddColumn<string>(
                name: "ReferenceType",
                table: "LoadingUnits",
                type: "char(1)",
                nullable: false,
                defaultValue: string.Empty);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReferenceType",
                table: "LoadingUnits");

            migrationBuilder.AddColumn<string>(
                name: "Reference",
                table: "LoadingUnits",
                type: "char(1)",
                nullable: false,
                defaultValue: string.Empty);
        }
    }
}
