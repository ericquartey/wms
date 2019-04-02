using Microsoft.EntityFrameworkCore.Migrations;

namespace Ferretto.Common.EF.Migrations
{
    [System.CodeDom.Compiler.GeneratedCode("EntityFramework", "v2.1")]
    public partial class Rename_Field_RequiredQuantity_For_Table_ItemListRows : Migration
    {
        #region Methods

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "RequestedQuantity",
                table: "ItemListRows",
                newName: "RequiredQuantity");
        }

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "RequiredQuantity",
                table: "ItemListRows",
                newName: "RequestedQuantity");
        }

        #endregion
    }
}
