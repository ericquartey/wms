using Microsoft.EntityFrameworkCore.Migrations;

namespace Ferretto.VW.MAS.DataLayer.Migrations
{
    public partial class ProfileConst : Migration
    {
        #region Methods

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProfileConst0",
                table: "Bays");

            migrationBuilder.DropColumn(
                name: "ProfileConst1",
                table: "Bays");
        }

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "ProfileConst0",
                table: "Bays",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "ProfileConst1",
                table: "Bays",
                nullable: false,
                defaultValue: 0.0);
        }

        #endregion
    }
}
