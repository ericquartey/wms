using Microsoft.EntityFrameworkCore.Migrations;

namespace Ferretto.VW.MAS.DataLayer.Migrations
{
    public partial class VeryHeavyPercent : Migration
    {
        #region Methods

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LoadUnitVeryHeavyPercent",
                table: "Machines");
        }

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "LoadUnitVeryHeavyPercent",
                table: "Machines",
                nullable: false,
                defaultValue: 85.0);

            migrationBuilder.Sql("UPDATE Machines SET LoadUnitVeryHeavyPercent = 85.0");
        }

        #endregion
    }
}
