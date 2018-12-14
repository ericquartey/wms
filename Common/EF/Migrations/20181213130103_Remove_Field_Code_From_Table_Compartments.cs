using Microsoft.EntityFrameworkCore.Migrations;

namespace Ferretto.Common.EF.Migrations
{
    public partial class Remove_Field_Code_From_Table_Compartments : Migration
    {
        #region Methods

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "Compartments",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Compartments_Code",
                table: "Compartments",
                column: "Code",
                unique: true,
                filter: "[Code] IS NOT NULL");
        }

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Compartments_Code",
                table: "Compartments");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "Compartments");
        }

        #endregion Methods
    }
}
