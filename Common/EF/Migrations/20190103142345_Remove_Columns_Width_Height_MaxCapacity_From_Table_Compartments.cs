using Microsoft.EntityFrameworkCore.Migrations;

namespace Ferretto.Common.EF.Migrations
{
    [System.CodeDom.Compiler.GeneratedCode("EntityFramework", "v2.1")]
    public partial class Remove_Columns_Width_Height_MaxCapacity_From_Table_Compartments : Migration
    {
        #region Methods

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            if (migrationBuilder == null)
            {
                throw new System.ArgumentNullException(nameof(migrationBuilder));
            }

            migrationBuilder.AddColumn<int>(
                name: "Height",
                table: "Compartments",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MaxCapacity",
                table: "Compartments",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Width",
                table: "Compartments",
                nullable: true);
        }

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            if (migrationBuilder == null)
            {
                throw new System.ArgumentNullException(nameof(migrationBuilder));
            }

            migrationBuilder.DropColumn(
                name: "Height",
                table: "Compartments");

            migrationBuilder.DropColumn(
                name: "MaxCapacity",
                table: "Compartments");

            migrationBuilder.DropColumn(
                name: "Width",
                table: "Compartments");
        }

        #endregion
    }
}
