using Microsoft.EntityFrameworkCore.Migrations;

namespace Ferretto.Common.EF.Migrations
{
    #pragma warning disable

    public partial class Rename_Machine_Fields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "MissionCount",
                table: "LoadingUnits",
                newName: "MissionsCount");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "MissionsCount",
                table: "LoadingUnits",
                newName: "MissionCount");
        }
    }

    #pragma warning restore
}
