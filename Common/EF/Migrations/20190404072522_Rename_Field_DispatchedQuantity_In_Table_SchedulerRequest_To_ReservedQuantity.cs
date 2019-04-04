using Microsoft.EntityFrameworkCore.Migrations;

namespace Ferretto.Common.EF.Migrations
{
    public partial class Rename_Field_DispatchedQuantity_In_Table_SchedulerRequest_To_ReservedQuantity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DispatchedQuantity",
                table: "SchedulerRequests",
                newName: "ReservedQuantity");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ReservedQuantity",
                table: "SchedulerRequests",
                newName: "DispatchedQuantity");
        }
    }
}
