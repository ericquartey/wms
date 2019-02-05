using Microsoft.EntityFrameworkCore.Migrations;

namespace Ferretto.Common.EF.Migrations
{
    public partial class RemoveDescriptionFromCompartmentType : Migration
    {
        #region Methods

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            if (migrationBuilder == null)
            {
                throw new System.ArgumentNullException(nameof(migrationBuilder));
            }

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "CompartmentTypes",
                nullable: false,
                defaultValue: "");
        }

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            if (migrationBuilder == null)
            {
                throw new System.ArgumentNullException(nameof(migrationBuilder));
            }

            migrationBuilder.DropColumn(
                name: "Description",
                table: "CompartmentTypes");
        }

        #endregion
    }
}
