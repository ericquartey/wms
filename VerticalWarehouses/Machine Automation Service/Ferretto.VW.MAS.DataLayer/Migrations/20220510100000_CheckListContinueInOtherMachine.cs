using Microsoft.EntityFrameworkCore.Migrations;

namespace Ferretto.VW.MAS.DataLayer.Migrations
{
    public partial class CheckListContinueInOtherMachine : Migration
    {
        #region Methods

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CheckListContinueInOtherMachine",
                table: "Bays");
        }

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "CheckListContinueInOtherMachine",
                table: "Bays",
                nullable: true,
                defaultValue: false);
        }

        #endregion
    }
}
