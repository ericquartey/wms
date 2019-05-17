using Microsoft.EntityFrameworkCore.Migrations;

namespace Ferretto.Common.EF.Migrations
{
    [System.CodeDom.Compiler.GeneratedCode("EntityFramework", "v2.2")]
    public partial class Rename_All_Store_In_Put_To_Table_Compartment_Item_LU : Migration
    {
        #region Methods

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "LastPutDate",
                table: "LoadingUnits",
                newName: "LastStoreDate");

            migrationBuilder.RenameColumn(
                name: "PutTolerance",
                table: "Items",
                newName: "StoreTolerance");

            migrationBuilder.RenameColumn(
                name: "LastPutDate",
                table: "Items",
                newName: "LastStoreDate");

            migrationBuilder.RenameColumn(
                name: "FifoTimePut",
                table: "Items",
                newName: "FifoTimeStore");

            migrationBuilder.RenameColumn(
                name: "ReservedToPut",
                table: "Compartments",
                newName: "ReservedToStore");

            migrationBuilder.RenameColumn(
                name: "LastPutDate",
                table: "Compartments",
                newName: "LastStoreDate");
        }

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "LastStoreDate",
                table: "LoadingUnits",
                newName: "LastPutDate");

            migrationBuilder.RenameColumn(
                name: "StoreTolerance",
                table: "Items",
                newName: "PutTolerance");

            migrationBuilder.RenameColumn(
                name: "LastStoreDate",
                table: "Items",
                newName: "LastPutDate");

            migrationBuilder.RenameColumn(
                name: "FifoTimeStore",
                table: "Items",
                newName: "FifoTimePut");

            migrationBuilder.RenameColumn(
                name: "ReservedToStore",
                table: "Compartments",
                newName: "ReservedToPut");

            migrationBuilder.RenameColumn(
                name: "LastStoreDate",
                table: "Compartments",
                newName: "LastPutDate");
        }

        #endregion
    }
}
