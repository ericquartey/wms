using Microsoft.EntityFrameworkCore.Migrations;

namespace Ferretto.Common.EF.Migrations
{
    [System.CodeDom.Compiler.GeneratedCode("EntityFramework", "v2.2")]
    public partial class Rename_Field_SchedulerType_To_Type_In_Table_SchedulerRequest : Migration
    {
        #region Methods

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Type",
                table: "SchedulerRequests");

            migrationBuilder.AddColumn<string>(
                name: "SchedulerType",
                table: "SchedulerRequests",
                type: "char(1)",
                nullable: false,
                defaultValue: string.Empty);

            migrationBuilder.AlterColumn<long>(
                name: "EmptyWeight",
                table: "LoadingUnitTypes",
                nullable: false,
                oldClrType: typeof(long),
                oldDefaultValue: 0L);
        }

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SchedulerType",
                table: "SchedulerRequests");

            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "SchedulerRequests",
                type: "char(1)",
                nullable: false,
                defaultValue: string.Empty);

            migrationBuilder.AlterColumn<long>(
                name: "EmptyWeight",
                table: "LoadingUnitTypes",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long));
        }

        #endregion
    }
}
