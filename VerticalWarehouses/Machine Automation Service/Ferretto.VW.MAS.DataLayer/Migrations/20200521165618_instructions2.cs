using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Ferretto.VW.MAS.DataLayer.Migrations
{
    public partial class instructions2 : Migration
    {
        #region Methods

        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable("InstructionDefinition");

            migrationBuilder.CreateTable(
                name: "InstructionDefinitions",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Axis = table.Column<int>(nullable: false, defaultValue: 0),
                    InstructionType = table.Column<int>(nullable: false),
                    Description = table.Column<string>(nullable: true),
                    CounterName = table.Column<string>(nullable: true),
                    MaxDays = table.Column<int>(nullable: true),
                    MaxRelativeCount = table.Column<int>(nullable: true),
                    MaxTotalCount = table.Column<int>(nullable: true),
                    BayNumber = table.Column<int>(nullable: false),
                    IsShutter = table.Column<bool>(nullable: false),
                    IsCarousel = table.Column<bool>(nullable: false),
                    IsSystem = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InstructionDefinitions", x => x.Id);
                });

            migrationBuilder.DropTable("Instruction");

            migrationBuilder.CreateTable(
                name: "Instructions",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DefinitionId = table.Column<int>(nullable: true),
                    DoubleCounter = table.Column<double>(nullable: true),
                    IntCounter = table.Column<int>(nullable: true),
                    IsDone = table.Column<bool>(nullable: false),
                    IsToDo = table.Column<bool>(nullable: false),
                    MaintenanceDate = table.Column<DateTime>(nullable: true),
                    ServicingInfoId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Instructions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Instructions_InstructionDefinitions_DefinitionId",
                        column: x => x.DefinitionId,
                        principalTable: "InstructionDefinitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Instructions_ServicingInfo_ServicingInfoId",
                        column: x => x.ServicingInfoId,
                        principalTable: "ServicingInfo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });
        }

        #endregion
    }
}
