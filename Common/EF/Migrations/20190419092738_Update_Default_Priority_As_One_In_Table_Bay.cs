using Microsoft.EntityFrameworkCore.Migrations;

namespace Ferretto.Common.EF.Migrations
{
    public partial class Update_Default_Priority_As_One_In_Table_Bay : Migration
    {
        #region Methods

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "Priority",
                table: "Bays",
                nullable: false,
                oldClrType: typeof(int),
                oldDefaultValueSql: "1");
        }

        [System.CodeDom.Compiler.GeneratedCode("EntityFramework", "v2.2")]
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "Priority",
                table: "Bays",
                nullable: false,
                defaultValueSql: "1",
                oldClrType: typeof(int));
        }

        #endregion
    }
}
