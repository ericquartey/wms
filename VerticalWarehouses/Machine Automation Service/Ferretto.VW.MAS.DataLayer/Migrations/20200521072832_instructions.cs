using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Ferretto.VW.MAS.DataLayer.Migrations
{
    public partial class instructions : Migration
    {
        #region Methods

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ServicingInfo_MachineStatistics_MachineStatisticsId",
                table: "ServicingInfo");

            migrationBuilder.DropTable(
                name: "Instruction");

            migrationBuilder.DropTable(
                name: "InstructionDefinition");

            migrationBuilder.DropIndex(
                name: "IX_ServicingInfo_MachineStatisticsId",
                table: "ServicingInfo");

            migrationBuilder.DropColumn(
                name: "MachineStatisticsId",
                table: "ServicingInfo");
        }

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable("ServicingInfo", newName: "ServicingInfo_old");
            migrationBuilder.CreateTable(
                name: "ServicingInfo",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    InstallationDate = table.Column<DateTime>(nullable: true),
                    LastServiceDate = table.Column<DateTime>(nullable: true),
                    MachineStatisticsId = table.Column<int>(nullable: true),
                    NextServiceDate = table.Column<DateTime>(nullable: true),
                    TotalMissions = table.Column<int>(nullable: true),
                    ServiceStatus = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServicingInfo", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "InstructionDefinition",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    InstructionType = table.Column<int>(nullable: false),
                    Description = table.Column<string>(nullable: true),
                    CounterName = table.Column<string>(nullable: true),
                    MaxDays = table.Column<int>(nullable: true),
                    MaxRelativeCount = table.Column<int>(nullable: true),
                    MaxTotalCount = table.Column<int>(nullable: true),
                    IsElevator = table.Column<bool>(nullable: false),
                    BayNumber = table.Column<int>(nullable: false),
                    IsShutter = table.Column<bool>(nullable: false),
                    IsCarousel = table.Column<bool>(nullable: false),
                    IsSystem = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InstructionDefinition", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Instruction",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DefinitionId = table.Column<int>(nullable: true),
                    IsDone = table.Column<bool>(nullable: false),
                    IsToDo = table.Column<bool>(nullable: false),
                    ServicingInfoId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Instruction", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Instruction_InstructionDefinition_DefinitionId",
                        column: x => x.DefinitionId,
                        principalTable: "InstructionDefinition",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Instruction_ServicingInfo_ServicingInfoId",
                        column: x => x.ServicingInfoId,
                        principalTable: "ServicingInfo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ServicingInfo_MachineStatisticsId",
                table: "ServicingInfo",
                column: "MachineStatisticsId");

            migrationBuilder.CreateIndex(
                name: "IX_Instruction_DefinitionId",
                table: "Instruction",
                column: "DefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_Instruction_ServicingInfoId",
                table: "Instruction",
                column: "ServicingInfoId");

            migrationBuilder.AddForeignKey(
                name: "FK_ServicingInfo_MachineStatistics_MachineStatisticsId",
                table: "ServicingInfo",
                column: "MachineStatisticsId",
                principalTable: "MachineStatistics",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.Sql("INSERT INTO ServicingInfo (InstallationDate, LastServiceDate, NextServiceDate, ServiceStatus) SELECT InstallationDate, LastServiceDate, NextServiceDate, ServiceStatus FROM ServicingInfo_old");

            migrationBuilder.DropTable("ServicingInfo_old");
        }

        #endregion
    }
}
