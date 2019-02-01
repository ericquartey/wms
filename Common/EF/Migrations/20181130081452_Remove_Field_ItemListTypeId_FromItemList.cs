using Microsoft.EntityFrameworkCore.Migrations;

namespace Ferretto.Common.EF.Migrations
{
    public partial class Remove_Field_ItemListTypeId_FromItemList : Migration
    {
        #region Methods

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            if (migrationBuilder == null)
            {
                throw new System.ArgumentNullException(nameof(migrationBuilder));
            }

            migrationBuilder.AddColumn<int>(
                name: "ItemListTypeId",
                table: "ItemLists",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            if (migrationBuilder == null)
            {
                throw new System.ArgumentNullException(nameof(migrationBuilder));
            }

            migrationBuilder.DropColumn(
                name: "ItemListTypeId",
                table: "ItemLists");
        }

        #endregion Methods
    }
}
