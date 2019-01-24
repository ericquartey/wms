using Microsoft.EntityFrameworkCore.Migrations;

namespace Ferretto.Common.EF.Migrations
{
    public partial class Add_Column_HasRotation_To_Table_Compartments : Migration
    {
        #region Methods

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            if (migrationBuilder == null)
            {
                throw new System.ArgumentNullException(nameof(migrationBuilder));
            }

            migrationBuilder.DropColumn(
                name: "HasRotation",
                table: "Compartments");
        }

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            if (migrationBuilder == null)
            {
                throw new System.ArgumentNullException(nameof(migrationBuilder));
            }

            migrationBuilder.AddColumn<bool>(
                name: "HasRotation",
                table: "Compartments",
                nullable: false,
                defaultValue: false);
        }

        #endregion Methods
    }
}
