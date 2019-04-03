using Microsoft.EntityFrameworkCore.Migrations;

namespace Ferretto.Common.EF.Migrations
{
    [System.CodeDom.Compiler.GeneratedCode("EntityFramework", "v2.2")]
    public partial class Add_Default_Value_To_ItemListRows_Status : Migration
    {
        #region Methods

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "ItemListRows",
                type: "char(1)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "char(1)",
                oldDefaultValueSql: "'N'");
        }

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "ItemListRows",
                type: "char(1)",
                nullable: false,
                defaultValueSql: "'N'",
                oldClrType: typeof(string),
                oldType: "char(1)");
        }

        #endregion
    }
}
