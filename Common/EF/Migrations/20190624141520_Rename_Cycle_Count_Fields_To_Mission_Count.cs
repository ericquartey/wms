using Microsoft.EntityFrameworkCore.Migrations;

namespace Ferretto.Common.EF.Migrations
{
    #pragma warning disable

    public partial class Rename_Cycle_Count_Fields_To_Mission_Count : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "OutCycleCount",
                table: "LoadingUnits",
                newName: "OutMissionCount");

            migrationBuilder.RenameColumn(
                name: "OtherCycleCount",
                table: "LoadingUnits",
                newName: "OtherMissionCount");

            migrationBuilder.RenameColumn(
                name: "InCycleCount",
                table: "LoadingUnits",
                newName: "InMissionCount");

            migrationBuilder.AlterColumn<double>(
                name: "MinStepCompartment",
                table: "GlobalSettings",
                nullable: false,
                oldClrType: typeof(double),
                oldDefaultValue: 5.0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "OutMissionCount",
                table: "LoadingUnits",
                newName: "OutCycleCount");

            migrationBuilder.RenameColumn(
                name: "OtherMissionCount",
                table: "LoadingUnits",
                newName: "OtherCycleCount");

            migrationBuilder.RenameColumn(
                name: "InMissionCount",
                table: "LoadingUnits",
                newName: "InCycleCount");

            migrationBuilder.AlterColumn<double>(
                name: "MinStepCompartment",
                table: "GlobalSettings",
                nullable: false,
                defaultValue: 5.0,
                oldClrType: typeof(double));
        }
    }

    #pragma warning restore
}
