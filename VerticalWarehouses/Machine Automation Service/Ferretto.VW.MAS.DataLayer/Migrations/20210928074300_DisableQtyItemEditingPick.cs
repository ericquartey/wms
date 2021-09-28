using Microsoft.EntityFrameworkCore.Migrations;

namespace Ferretto.VW.MAS.DataLayer.Migrations
{
    public partial class DisableQtyItemEditingPick : Migration
    {
        #region Methods

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDisableQtyItemEditingPick",
                table: "Machines");
        }

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDisableQtyItemEditingPick",
                table: "Machines",
                nullable: false,
                defaultValue: false);
        }

        #endregion
    }
}
