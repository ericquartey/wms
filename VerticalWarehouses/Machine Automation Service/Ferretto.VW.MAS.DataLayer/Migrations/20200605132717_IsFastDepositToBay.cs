using Microsoft.EntityFrameworkCore.Migrations;

namespace Ferretto.VW.MAS.DataLayer.Migrations
{
    public partial class IsFastDepositToBay : Migration
    {
        #region Methods

        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsFastDepositToBay",
                table: "Bays",
                nullable: false,
                defaultValue: false);
        }

        #endregion
    }
}
