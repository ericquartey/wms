using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Ferretto.VW.MAS_DataLayer.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ConfigurationValues",
                columns: table => new
                {
                    CategoryName = table.Column<long>(nullable: false),
                    VarName = table.Column<long>(nullable: false),
                    VarType = table.Column<long>(nullable: false),
                    VarValue = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConfigurationValues", x => new { x.CategoryName, x.VarName });
                });

            migrationBuilder.CreateTable(
                name: "LoadingUnits",
                columns: table => new
                {
                    CellPosition = table.Column<decimal>(nullable: false),
                    Height = table.Column<decimal>(nullable: false),
                    LoadingUnitId = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Status = table.Column<long>(nullable: false),
                    Weight = table.Column<decimal>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoadingUnits", x => x.LoadingUnitId);
                });

            migrationBuilder.CreateTable(
                name: "LogEntries",
                columns: table => new
                {
                    Data = table.Column<string>(nullable: true),
                    Description = table.Column<string>(nullable: true),
                    Destination = table.Column<string>(nullable: true),
                    ErrorLevel = table.Column<string>(nullable: true),
                    Exception = table.Column<string>(nullable: true),
                    Level = table.Column<string>(nullable: true),
                    LogEntryID = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    LoggerName = table.Column<string>(nullable: true),
                    Message = table.Column<string>(nullable: true),
                    Source = table.Column<string>(nullable: true),
                    Status = table.Column<string>(nullable: true),
                    TimeStamp = table.Column<DateTime>(nullable: false),
                    Type = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LogEntries", x => x.LogEntryID);
                });

            migrationBuilder.CreateTable(
                name: "RuntimeValues",
                columns: table => new
                {
                    VarName = table.Column<long>(nullable: false),
                    VarType = table.Column<long>(nullable: false),
                    VarValue = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RuntimeValues", x => x.VarName);
                });

            migrationBuilder.CreateTable(
                name: "Cells",
                columns: table => new
                {
                    CellId = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Coord = table.Column<decimal>(nullable: false),
                    LoadingUnitId = table.Column<int>(nullable: false),
                    Priority = table.Column<int>(nullable: false),
                    Side = table.Column<long>(nullable: false),
                    Status = table.Column<long>(nullable: false),
                    WorkingStatus = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cells", x => x.CellId);
                    table.ForeignKey(
                        name: "FK_Cells_LoadingUnits_LoadingUnitId",
                        column: x => x.LoadingUnitId,
                        principalTable: "LoadingUnits",
                        principalColumn: "LoadingUnitId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FreeBlocks",
                columns: table => new
                {
                    BlockSize = table.Column<int>(nullable: false),
                    BookedCellsNumber = table.Column<int>(nullable: false),
                    Coord = table.Column<decimal>(nullable: false),
                    FreeBlockId = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    LoadingUnitId = table.Column<int>(nullable: false),
                    Priority = table.Column<int>(nullable: false),
                    Side = table.Column<long>(nullable: false),
                    StartCell = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FreeBlocks", x => x.FreeBlockId);
                    table.ForeignKey(
                        name: "FK_FreeBlocks_LoadingUnits_LoadingUnitId",
                        column: x => x.LoadingUnitId,
                        principalTable: "LoadingUnits",
                        principalColumn: "LoadingUnitId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "ConfigurationValues",
                columns: new[] { "CategoryName", "VarName", "VarType", "VarValue" },
                values: new object[] { 0L, 2L, 5L, "169.254.231.248" });

            migrationBuilder.InsertData(
                table: "ConfigurationValues",
                columns: new[] { "CategoryName", "VarName", "VarType", "VarValue" },
                values: new object[] { 0L, 17L, 2L, "17221" });

            migrationBuilder.InsertData(
                table: "ConfigurationValues",
                columns: new[] { "CategoryName", "VarName", "VarType", "VarValue" },
                values: new object[] { 0L, 4L, 5L, "169.254.231.10" });

            migrationBuilder.InsertData(
                table: "ConfigurationValues",
                columns: new[] { "CategoryName", "VarName", "VarType", "VarValue" },
                values: new object[] { 0L, 19L, 2L, "502" });

            migrationBuilder.CreateIndex(
                name: "IX_Cells_LoadingUnitId",
                table: "Cells",
                column: "LoadingUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_FreeBlocks_LoadingUnitId",
                table: "FreeBlocks",
                column: "LoadingUnitId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Cells");

            migrationBuilder.DropTable(
                name: "ConfigurationValues");

            migrationBuilder.DropTable(
                name: "FreeBlocks");

            migrationBuilder.DropTable(
                name: "LogEntries");

            migrationBuilder.DropTable(
                name: "RuntimeValues");

            migrationBuilder.DropTable(
                name: "LoadingUnits");
        }
    }
}
