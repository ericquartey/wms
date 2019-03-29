using Microsoft.EntityFrameworkCore.Migrations;

namespace Ferretto.Common.EF.Migrations
{
    [System.CodeDom.Compiler.GeneratedCode("EntityFramework", "v2.1")]
    public partial class Rename_Field_RequiredQuantity_From_Table_Missions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "RequiredQuantity",
                table: "Missions",
                newName: "RequestedQuantity");

            migrationBuilder.AddColumn<int>(
                name: "DispatchedQuantity",
                table: "Missions",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<bool>(
                name: "IsCellPairingFixed",
                table: "LoadingUnits",
                nullable: false,
                oldClrType: typeof(bool),
                oldNullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DispatchedQuantity",
                table: "Missions");

            migrationBuilder.RenameColumn(
                name: "RequestedQuantity",
                table: "Missions",
                newName: "RequiredQuantity");

            migrationBuilder.AlterColumn<bool>(
                name: "IsCellPairingFixed",
                table: "LoadingUnits",
                nullable: true,
                oldClrType: typeof(bool));
        }
    }
}
