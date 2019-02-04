using Microsoft.EntityFrameworkCore.Migrations;

namespace Ferretto.Common.EF.Migrations
{
    public partial class Remove_Column_AreaID_From_ItemList : Migration
    {
        #region Methods

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            if (migrationBuilder == null)
            {
                throw new System.ArgumentNullException(nameof(migrationBuilder));
            }

            migrationBuilder.AddColumn<int>(
                name: "AreaId",
                table: "ItemLists",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_ItemLists_AreaId",
                table: "ItemLists",
                column: "AreaId");

            migrationBuilder.AddForeignKey(
                name: "FK_ItemLists_Areas_AreaId",
                table: "ItemLists",
                column: "AreaId",
                principalTable: "Areas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            if (migrationBuilder == null)
            {
                throw new System.ArgumentNullException(nameof(migrationBuilder));
            }

            migrationBuilder.DropForeignKey(
                name: "FK_ItemLists_Areas_AreaId",
                table: "ItemLists");

            migrationBuilder.DropIndex(
                name: "IX_ItemLists_AreaId",
                table: "ItemLists");

            migrationBuilder.DropColumn(
                name: "AreaId",
                table: "ItemLists");
        }

        #endregion
    }
}
