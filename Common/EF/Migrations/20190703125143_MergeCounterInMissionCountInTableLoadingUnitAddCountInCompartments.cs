using Microsoft.EntityFrameworkCore.Migrations;

namespace Ferretto.Common.EF.Migrations
{
    [System.CodeDom.Compiler.GeneratedCode("EntityFramework", "v2.2")]
    public partial class MergeCounterInMissionCountInTableLoadingUnitAddCountInCompartments : Migration
    {
        #region Methods

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OtherMissionOperationCount",
                table: "Compartments");

            migrationBuilder.DropColumn(
                name: "PickMissionOperationCount",
                table: "Compartments");

            migrationBuilder.DropColumn(
                name: "PutMissionOperationCount",
                table: "Compartments");

            migrationBuilder.RenameColumn(
                name: "MissionCount",
                table: "LoadingUnits",
                newName: "OutMissionCount");

            migrationBuilder.AddColumn<int>(
                name: "InMissionCount",
                table: "LoadingUnits",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "OtherMissionCount",
                table: "LoadingUnits",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InMissionCount",
                table: "LoadingUnits");

            migrationBuilder.DropColumn(
                name: "OtherMissionCount",
                table: "LoadingUnits");

            migrationBuilder.RenameColumn(
                name: "OutMissionCount",
                table: "LoadingUnits",
                newName: "MissionCount");

            migrationBuilder.AddColumn<int>(
                name: "OtherMissionOperationCount",
                table: "Compartments",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PickMissionOperationCount",
                table: "Compartments",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PutMissionOperationCount",
                table: "Compartments",
                nullable: false,
                defaultValue: 0);
        }

        #endregion
    }
}
