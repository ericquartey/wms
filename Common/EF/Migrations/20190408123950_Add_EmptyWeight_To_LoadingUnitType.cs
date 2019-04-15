using Microsoft.EntityFrameworkCore.Migrations;

namespace Ferretto.Common.EF.Migrations
{
    [System.CodeDom.Compiler.GeneratedCode("EntityFramework", "v2.2")]
    public partial class Add_EmptyWeight_To_LoadingUnitType : Migration
    {
        #region Methods

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EmptyWeight",
                table: "LoadingUnitTypes");
        }

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "EmptyWeight",
                table: "LoadingUnitTypes",
                nullable: false,
                defaultValue: 0L);
        }

        #endregion
    }
}
