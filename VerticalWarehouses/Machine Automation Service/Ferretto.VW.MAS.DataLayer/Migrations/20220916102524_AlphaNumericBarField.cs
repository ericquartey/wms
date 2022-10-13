using Microsoft.EntityFrameworkCore.Migrations;

namespace Ferretto.VW.MAS.DataLayer.Migrations
{
    public partial class AlphaNumericBarField : Migration
    {
        #region Methods

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Field1",
                table: "Accessories");

            migrationBuilder.DropColumn(
                name: "Field2",
                table: "Accessories");

            migrationBuilder.DropColumn(
                name: "Field3",
                table: "Accessories");

            migrationBuilder.DropColumn(
                name: "Field4",
                table: "Accessories");

            migrationBuilder.DropColumn(
                name: "Field5",
                table: "Accessories");

            migrationBuilder.DropColumn(
                name: "UseGet",
                table: "Accessories");
        }

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Field1",
                table: "Accessories",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Field2",
                table: "Accessories",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Field3",
                table: "Accessories",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Field4",
                table: "Accessories",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Field5",
                table: "Accessories",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "UseGet",
                table: "Accessories",
                nullable: true);
        }

        #endregion
    }
}
