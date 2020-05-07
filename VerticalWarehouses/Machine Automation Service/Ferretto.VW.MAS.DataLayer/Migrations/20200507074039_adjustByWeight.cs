using Microsoft.EntityFrameworkCore.Migrations;

namespace Ferretto.VW.MAS.DataLayer.Migrations
{
    public partial class adjustByWeight : Migration
    {
        #region Methods

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AdjustAccelerationByWeight",
                table: "MovementParameters");

            migrationBuilder.RenameColumn(
                name: "AdjustSpeedByWeight",
                table: "MovementParameters",
                newName: "AdjustByWeight");
        }

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "AdjustByWeight",
                table: "MovementParameters",
                newName: "AdjustSpeedByWeight");

            migrationBuilder.AddColumn<bool>(
                name: "AdjustAccelerationByWeight",
                table: "MovementParameters",
                nullable: true);

            migrationBuilder.Sql("UPDATE MovementParameters SET AdjustAccelerationByWeight = AdjustSpeedByWeight");
        }

        #endregion
    }
}
