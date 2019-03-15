using Microsoft.EntityFrameworkCore.Migrations;

namespace Ferretto.Common.EF.Migrations
{
    public partial class RemoveRequiredToCellPairingInLoadingUnit : Migration
    {
        [System.CodeDom.Compiler.GeneratedCode("EntityFramework", "v2.1")]
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<bool>(
                name: "IsCellPairingFixed",
                table: "LoadingUnits",
                nullable: true,
                oldClrType: typeof(bool));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<bool>(
                name: "IsCellPairingFixed",
                table: "LoadingUnits",
                nullable: false,
                oldClrType: typeof(bool),
                oldNullable: true);
        }
    }
}
