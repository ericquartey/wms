using Microsoft.EntityFrameworkCore.Migrations;

namespace Ferretto.Common.EF.Migrations
{
    [System.CodeDom.Compiler.GeneratedCode("EntityFramework", "v2.2")]
    public partial class Rename_Field_DispatchedQuantity_In_Table_SchedulerRequest_To_ReservedQuantity : Migration
    {
        #region Methods

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ReservedQuantity",
                table: "SchedulerRequests",
                newName: "DispatchedQuantity");
        }

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DispatchedQuantity",
                table: "SchedulerRequests",
                newName: "ReservedQuantity");
        }

        #endregion
    }
}
