using Microsoft.EntityFrameworkCore.Migrations;

namespace Ferretto.Common.EF.Migrations
{
    [System.CodeDom.Compiler.GeneratedCode("EntityFramework", "v2.2")]
    public partial class Add_Priority_Field_To_Tables_SchedulerRequests_ItemLists_ItemListRows : Migration
    {
        #region Methods

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Priority",
                table: "SchedulerRequests");

            migrationBuilder.DropColumn(
                name: "Priority",
                table: "Bays");

            migrationBuilder.AlterColumn<int>(
                name: "Priority",
                table: "ItemLists",
                nullable: false,
                defaultValue: 1,
                oldClrType: typeof(int),
                oldNullable: true,
                oldDefaultValue: 1);

            migrationBuilder.AlterColumn<int>(
                name: "Priority",
                table: "ItemListRows",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);
        }

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Priority",
                table: "SchedulerRequests",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Priority",
                table: "ItemLists",
                nullable: true,
                defaultValue: 1,
                oldClrType: typeof(int),
                oldDefaultValue: 1);

            migrationBuilder.AlterColumn<int>(
                name: "Priority",
                table: "ItemListRows",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.AddColumn<int>(
                name: "Priority",
                table: "Bays",
                nullable: false,
                defaultValue: 0);
        }

        #endregion
    }
}
