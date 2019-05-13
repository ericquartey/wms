using Microsoft.EntityFrameworkCore.Migrations;

namespace Ferretto.Common.EF.Migrations
{
    [System.CodeDom.Compiler.GeneratedCode("EntityFramework", "v2.2")]
    public partial class Remove_And_Rename_Date_Fifo_To_Table_Compartment : Migration
    {
        #region Methods

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "FifoStartDate",
                table: "Compartments",
                newName: "FirstStoreDate");

            migrationBuilder.AddColumn<int>(
                name: "FifoTime",
                table: "Compartments",
                nullable: true);
        }

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FifoTime",
                table: "Compartments");

            migrationBuilder.RenameColumn(
                name: "FirstStoreDate",
                table: "Compartments",
                newName: "FifoStartDate");
        }

        #endregion
    }
}
