using Microsoft.EntityFrameworkCore.Migrations;

namespace Ferretto.VW.MAS.DataLayer.Migrations
{
    public partial class OperationsBay : Migration
    {
        #region Methods

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Inventory",
                table: "Bays");

            migrationBuilder.DropColumn(
                name: "Pick",
                table: "Bays");

            migrationBuilder.DropColumn(
                name: "Put",
                table: "Bays");

            migrationBuilder.DropColumn(
                name: "View",
                table: "Bays");
        }

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Inventory",
                table: "Bays",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "Pick",
                table: "Bays",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "Put",
                table: "Bays",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "View",
                table: "Bays",
                nullable: false,
                defaultValue: true);

            migrationBuilder.Sql("UPDATE Bays SET Inventory = 1");
            migrationBuilder.Sql("UPDATE Bays SET Pick = 1");
            migrationBuilder.Sql("UPDATE Bays SET Put = 1");
            migrationBuilder.Sql("UPDATE Bays SET View = 1");
        }

        #endregion
    }
}
