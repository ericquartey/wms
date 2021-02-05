using Microsoft.EntityFrameworkCore.Migrations;

namespace Ferretto.VW.MAS.DataLayer.Migrations
{
    public partial class ShutterMaxMinSpeed : Migration
    {
        #region Methods

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MaxSpeed",
                table: "Shutters");

            migrationBuilder.DropColumn(
                name: "MinSpeed",
                table: "Shutters");
        }

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "MaxSpeed",
                table: "Shutters",
                nullable: false,
                defaultValue: 40000.0);

            migrationBuilder.AddColumn<double>(
                name: "MinSpeed",
                table: "Shutters",
                nullable: false,
                defaultValue: 5000.0);
        }

        #endregion
    }
}
