using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Ferretto.Common.EF.Migrations
{
    [System.CodeDom.Compiler.GeneratedCode("EntityFramework", "v2.1")]
    public partial class Remove_Field_LastHandlingDate_From_Table_Compartments : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastHandlingDate",
                table: "Compartments");

            migrationBuilder.AlterColumn<bool>(
                name: "IsCellPairingFixed",
                table: "LoadingUnits",
                nullable: false,
                oldClrType: typeof(bool),
                oldNullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<bool>(
                name: "IsCellPairingFixed",
                table: "LoadingUnits",
                nullable: true,
                oldClrType: typeof(bool));

            migrationBuilder.AddColumn<DateTime>(
                name: "LastHandlingDate",
                table: "Compartments",
                nullable: true);
        }
    }
}
