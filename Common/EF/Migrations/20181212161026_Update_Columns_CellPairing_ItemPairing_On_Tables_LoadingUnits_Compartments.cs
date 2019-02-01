using Microsoft.EntityFrameworkCore.Migrations;

namespace Ferretto.Common.EF.Migrations
{
    public partial class Update_Columns_CellPairing_ItemPairing_On_Tables_LoadingUnits_Compartments : Migration
    {
        #region Methods

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            if (migrationBuilder == null)
            {
                throw new System.ArgumentNullException(nameof(migrationBuilder));
            }

            migrationBuilder.DropColumn(
                name: "IsCellPairingFixed",
                table: "LoadingUnits");

            migrationBuilder.DropColumn(
                name: "IsCellPairingFixed",
                table: "DefaultLoadingUnits");

            migrationBuilder.DropColumn(
                name: "IsItemPairingFixed",
                table: "Compartments");

            migrationBuilder.AddColumn<string>(
                name: "CellPairing",
                table: "LoadingUnits",
                type: "NVARCHAR(MAX)",
                nullable: false,
                defaultValue: "Free");

            migrationBuilder.AddColumn<string>(
                name: "CellPairing",
                table: "DefaultLoadingUnits",
                type: "NVARCHAR(MAX)",
                nullable: false,
                defaultValue: "Free");

            migrationBuilder.AddColumn<string>(
                name: "ItemPairing",
                table: "Compartments",
                type: "NVARCHAR(MAX)",
                nullable: false,
                defaultValue: "Free");
        }

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            if (migrationBuilder == null)
            {
                throw new System.ArgumentNullException(nameof(migrationBuilder));
            }

            migrationBuilder.DropColumn(
                name: "CellPairing",
                table: "LoadingUnits");

            migrationBuilder.DropColumn(
                name: "CellPairing",
                table: "DefaultLoadingUnits");

            migrationBuilder.DropColumn(
                name: "ItemPairing",
                table: "Compartments");

            migrationBuilder.AddColumn<bool>(
                name: "IsCellPairingFixed",
                table: "LoadingUnits",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsCellPairingFixed",
                table: "DefaultLoadingUnits",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsItemPairingFixed",
                table: "Compartments",
                nullable: false,
                defaultValue: false);
        }

        #endregion Methods
    }
}
