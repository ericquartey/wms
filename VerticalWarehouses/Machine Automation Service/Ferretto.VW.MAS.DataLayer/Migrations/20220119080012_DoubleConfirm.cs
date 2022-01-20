using Microsoft.EntityFrameworkCore.Migrations;

namespace Ferretto.VW.MAS.DataLayer.Migrations
{
    public partial class DoubleConfirm : Migration
    {
        #region Methods

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDoubleConfirmBarcodeInventory",
                table: "Machines");

            migrationBuilder.DropColumn(
                name: "IsDoubleConfirmBarcodePick",
                table: "Machines");

            migrationBuilder.DropColumn(
                name: "IsDoubleConfirmBarcodePut",
                table: "Machines");
        }

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDoubleConfirmBarcodeInventory",
                table: "Machines",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDoubleConfirmBarcodePick",
                table: "Machines",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDoubleConfirmBarcodePut",
                table: "Machines",
                nullable: false,
                defaultValue: false);
        }

        #endregion
    }
}
