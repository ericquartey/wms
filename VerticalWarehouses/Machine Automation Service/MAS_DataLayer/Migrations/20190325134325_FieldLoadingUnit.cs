using Microsoft.EntityFrameworkCore.Migrations;

namespace Ferretto.VW.MAS_DataLayer.Migrations
{
    public partial class FieldLoadingUnit : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_RuntimeValues",
                table: "RuntimeValues");

            migrationBuilder.AddColumn<long>(
                name: "CategoryName",
                table: "RuntimeValues",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "LoadingUnits",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MaxWeight",
                table: "LoadingUnits",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddPrimaryKey(
                name: "PK_RuntimeValues",
                table: "RuntimeValues",
                columns: new[] { "CategoryName", "VarName" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_RuntimeValues",
                table: "RuntimeValues");

            migrationBuilder.DropColumn(
                name: "CategoryName",
                table: "RuntimeValues");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "LoadingUnits");

            migrationBuilder.DropColumn(
                name: "MaxWeight",
                table: "LoadingUnits");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RuntimeValues",
                table: "RuntimeValues",
                column: "VarName");
        }
    }
}
