using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Ferretto.Common.EF.Migrations
{
    public partial class Remove_Table_ItemListType : Migration
    {
        #region Methods

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            if (migrationBuilder == null)
            {
                throw new System.ArgumentNullException(nameof(migrationBuilder));
            }

            migrationBuilder.DropColumn(
                name: "ItemListType",
                table: "ItemLists");

            migrationBuilder.CreateTable(
                name: "ItemListTypes",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Description = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemListTypes", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ItemLists_ItemListTypeId",
                table: "ItemLists",
                column: "ItemListTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_ItemLists_ItemListTypes_ItemListTypeId",
                table: "ItemLists",
                column: "ItemListTypeId",
                principalTable: "ItemListTypes",
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
                name: "FK_ItemLists_ItemListTypes_ItemListTypeId",
                table: "ItemLists");

            migrationBuilder.DropTable(
                name: "ItemListTypes");

            migrationBuilder.DropIndex(
                name: "IX_ItemLists_ItemListTypeId",
                table: "ItemLists");

            migrationBuilder.AddColumn<string>(
                name: "ItemListType",
                table: "ItemLists",
                type: "char(1)",
                nullable: false,
                defaultValue: string.Empty);
        }

        #endregion
    }
}
