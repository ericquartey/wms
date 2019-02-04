using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Ferretto.Common.EF.Migrations
{
    public partial class Remove_Table_ItemListRowType : Migration
    {
        #region Methods

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            if (migrationBuilder == null)
            {
                throw new System.ArgumentNullException(nameof(migrationBuilder));
            }

            migrationBuilder.DropColumn(
                name: "ItemListRowStatus",
                table: "ItemListRows");

            migrationBuilder.AddColumn<int>(
                name: "ItemListRowStatusId",
                table: "ItemListRows",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "ItemListRowStatuses",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Description = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemListRowStatuses", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ItemListRows_ItemListRowStatusId",
                table: "ItemListRows",
                column: "ItemListRowStatusId");

            migrationBuilder.AddForeignKey(
                name: "FK_ItemListRows_ItemListRowStatuses_ItemListRowStatusId",
                table: "ItemListRows",
                column: "ItemListRowStatusId",
                principalTable: "ItemListRowStatuses",
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
                name: "FK_ItemListRows_ItemListRowStatuses_ItemListRowStatusId",
                table: "ItemListRows");

            migrationBuilder.DropTable(
                name: "ItemListRowStatuses");

            migrationBuilder.DropIndex(
                name: "IX_ItemListRows_ItemListRowStatusId",
                table: "ItemListRows");

            migrationBuilder.DropColumn(
                name: "ItemListRowStatusId",
                table: "ItemListRows");

            migrationBuilder.AddColumn<string>(
                name: "ItemListRowStatus",
                table: "ItemListRows",
                type: "char(1)",
                nullable: false,
                defaultValue: "");
        }

        #endregion
    }
}
