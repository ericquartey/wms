using Microsoft.EntityFrameworkCore.Migrations;

namespace Ferretto.VW.MAS.DataLayer.Migrations
{
    public partial class BoxAndItemUniqueIdLength : Migration
    {
        #region Methods

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Box",
                table: "Machines");

            migrationBuilder.DropColumn(
                name: "ItemUniqueIdLength",
                table: "Machines");
        }

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Box",
                table: "Machines",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "ItemUniqueIdLength",
                table: "Machines",
                nullable: false,
                defaultValue: 0);
        }

        #endregion
    }
}
