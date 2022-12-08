using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Ferretto.VW.MAS.DataLayer.Migrations
{
    public partial class InverterStat : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "TotalInverterMissionTime",
                table: "MachineStatistics",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "TotalInverterPowerOnTime",
                table: "MachineStatistics",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.CreateTable(
                name: "InverterStatistics",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AverageActivePower = table.Column<double>(nullable: false),
                    AverageRMSCurrent = table.Column<double>(nullable: false),
                    DateTime = table.Column<DateTimeOffset>(nullable: false),
                    PeakHeatSinkTemperature = table.Column<double>(nullable: false),
                    PeakInsideTemperature = table.Column<double>(nullable: false),
                    MachineStatisticsId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InverterStatistics", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InverterStatistics_MachineStatistics_MachineStatisticsId",
                        column: x => x.MachineStatisticsId,
                        principalTable: "MachineStatistics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InverterStatistics_MachineStatisticsId",
                table: "InverterStatistics",
                column: "MachineStatisticsId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InverterStatistics");

            migrationBuilder.DropColumn(
                name: "TotalInverterMissionTime",
                table: "MachineStatistics");

            migrationBuilder.DropColumn(
                name: "TotalInverterPowerOnTime",
                table: "MachineStatistics");
        }
    }
}
