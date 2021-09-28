using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Ferretto.VW.TelemetryService.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Machines",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ModelName = table.Column<string>(nullable: false),
                    RawDatabaseContent = table.Column<byte[]>(nullable: true),
                    SerialNumber = table.Column<string>(nullable: false),
                    Version = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Machines", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ServicingInfos",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    InstallationDate = table.Column<DateTimeOffset>(nullable: true),
                    IsHandOver = table.Column<bool>(nullable: false),
                    LastServiceDate = table.Column<DateTimeOffset>(nullable: true),
                    NextServiceDate = table.Column<DateTimeOffset>(nullable: true),
                    ServiceStatusId = table.Column<int>(nullable: false),
                    TimeStamp = table.Column<DateTimeOffset>(nullable: false),
                    TotalMissions = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServicingInfos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErrorLogs",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AdditionalText = table.Column<string>(nullable: true),
                    BayNumber = table.Column<int>(nullable: false),
                    Code = table.Column<int>(nullable: false),
                    DetailCode = table.Column<int>(nullable: false),
                    InverterIndex = table.Column<int>(nullable: false),
                    MachineId = table.Column<int>(nullable: false),
                    OccurrenceDate = table.Column<DateTimeOffset>(nullable: false),
                    ResolutionDate = table.Column<DateTimeOffset>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ErrorLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ErrorLogs_Machines_MachineId",
                        column: x => x.MachineId,
                        principalTable: "Machines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "IOLogs",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    BayNumber = table.Column<int>(nullable: false),
                    Description = table.Column<string>(nullable: true),
                    Input = table.Column<string>(nullable: true),
                    MachineId = table.Column<int>(nullable: true),
                    Output = table.Column<string>(nullable: true),
                    TimeStamp = table.Column<DateTimeOffset>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IOLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IOLogs_Machines_MachineId",
                        column: x => x.MachineId,
                        principalTable: "Machines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MissionLogs",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Bay = table.Column<int>(nullable: false),
                    CellId = table.Column<int>(nullable: true),
                    CreationDate = table.Column<DateTimeOffset>(nullable: false),
                    Destination = table.Column<string>(nullable: true),
                    Direction = table.Column<int>(nullable: false),
                    EjectLoadUnit = table.Column<int>(nullable: false),
                    LoadUnitId = table.Column<int>(nullable: false),
                    MachineId = table.Column<int>(nullable: false),
                    MissionId = table.Column<int>(nullable: false),
                    MissionType = table.Column<string>(nullable: true),
                    Priority = table.Column<int>(nullable: false),
                    Stage = table.Column<string>(nullable: false),
                    Status = table.Column<string>(nullable: true),
                    Step = table.Column<int>(nullable: false),
                    StopReason = table.Column<int>(nullable: false),
                    TimeStamp = table.Column<DateTimeOffset>(nullable: false),
                    WmsId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MissionLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MissionLogs_Machines_MachineId",
                        column: x => x.MachineId,
                        principalTable: "Machines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ScreenShots",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    BayNumber = table.Column<int>(nullable: false),
                    Image = table.Column<byte[]>(nullable: true),
                    MachineId = table.Column<int>(nullable: false),
                    TimeStamp = table.Column<DateTimeOffset>(nullable: false),
                    ViewName = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScreenShots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ScreenShots_Machines_MachineId",
                        column: x => x.MachineId,
                        principalTable: "Machines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ErrorLogs_MachineId",
                table: "ErrorLogs",
                column: "MachineId");

            migrationBuilder.CreateIndex(
                name: "IX_IOLogs_MachineId",
                table: "IOLogs",
                column: "MachineId");

            migrationBuilder.CreateIndex(
                name: "IX_MissionLogs_MachineId",
                table: "MissionLogs",
                column: "MachineId");

            migrationBuilder.CreateIndex(
                name: "IX_ScreenShots_MachineId",
                table: "ScreenShots",
                column: "MachineId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ErrorLogs");

            migrationBuilder.DropTable(
                name: "IOLogs");

            migrationBuilder.DropTable(
                name: "MissionLogs");

            migrationBuilder.DropTable(
                name: "ScreenShots");

            migrationBuilder.DropTable(
                name: "ServicingInfos");

            migrationBuilder.DropTable(
                name: "Machines");
        }
    }
}
