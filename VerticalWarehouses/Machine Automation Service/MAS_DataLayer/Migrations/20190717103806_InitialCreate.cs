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
                name: "Errors",
                columns: table => new
                {
                    Code = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Description = table.Column<string>(nullable: false),
                    Issue = table.Column<int>(nullable: false),
                    Reason = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Errors", x => x.Code);
                });

            migrationBuilder.CreateTable(
                name: "LoadingUnits",
                columns: table => new
                {
                    CellPosition = table.Column<decimal>(nullable: false),
                    Description = table.Column<string>(nullable: true),
                    Height = table.Column<decimal>(nullable: false),
                    LoadingUnitId = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    MaxWeight = table.Column<decimal>(nullable: false),
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
                name: "MachineStatistics",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TotalVerticalAxisCycles = table.Column<int>(nullable: false),
                    TotalVerticalAxisKilometers = table.Column<double>(nullable: false),
                    TotalBeltCycles = table.Column<int>(nullable: false),
                    TotalShutter1Cycles = table.Column<int>(nullable: false),
                    TotalShutter2Cycles = table.Column<int>(nullable: false),
                    TotalShutter3Cycles = table.Column<int>(nullable: false),
                    TotalMovedTraysInBay1 = table.Column<int>(nullable: false),
                    TotalMovedTraysInBay2 = table.Column<int>(nullable: false),
                    TotalMovedTraysInBay3 = table.Column<int>(nullable: false),
                    TotalMovedTrays = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MachineStatistics", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RuntimeValues",
                columns: table => new
                {
                    CategoryName = table.Column<long>(nullable: false),
                    VarName = table.Column<long>(nullable: false),
                    VarType = table.Column<long>(nullable: false),
                    VarValue = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RuntimeValues", x => new { x.CategoryName, x.VarName });
                });

            migrationBuilder.CreateTable(
                name: "ErrorStatistics",
                columns: table => new
                {
                    Code = table.Column<int>(nullable: false),
                    TotalErrors = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ErrorStatistics", x => x.Code);
                    table.ForeignKey(
                        name: "FK_ErrorStatistics_Errors_Code",
                        column: x => x.Code,
                        principalTable: "Errors",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Restrict);
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
                    Side = table.Column<int>(nullable: false),
                    Status = table.Column<int>(nullable: false),
                    WorkingStatus = table.Column<int>(nullable: false)
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
                    Side = table.Column<int>(nullable: false),
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
                table: "Errors",
                columns: new[] { "Code", "Description", "Issue", "Reason" },
                values: new object[] { 1001, "Errore database", 5, null });

            migrationBuilder.InsertData(
                table: "Errors",
                columns: new[] { "Code", "Description", "Issue", "Reason" },
                values: new object[] { 1016, "Errore rientro baia", 5, null });

            migrationBuilder.InsertData(
                table: "Errors",
                columns: new[] { "Code", "Description", "Issue", "Reason" },
                values: new object[] { 1015, "Errore rientro baia", 5, null });

            migrationBuilder.InsertData(
                table: "Errors",
                columns: new[] { "Code", "Description", "Issue", "Reason" },
                values: new object[] { 1014, "Errore rientro baia", 5, null });

            migrationBuilder.InsertData(
                table: "Errors",
                columns: new[] { "Code", "Description", "Issue", "Reason" },
                values: new object[] { 1013, "Errore rientro baia", 5, null });

            migrationBuilder.InsertData(
                table: "Errors",
                columns: new[] { "Code", "Description", "Issue", "Reason" },
                values: new object[] { 1012, "Errore rientro baia", 5, null });

            migrationBuilder.InsertData(
                table: "Errors",
                columns: new[] { "Code", "Description", "Issue", "Reason" },
                values: new object[] { 1011, "Errore rientro baia", 5, null });

            migrationBuilder.InsertData(
                table: "Errors",
                columns: new[] { "Code", "Description", "Issue", "Reason" },
                values: new object[] { 1010, "Errore rientro baia", 5, null });

            migrationBuilder.InsertData(
                table: "Errors",
                columns: new[] { "Code", "Description", "Issue", "Reason" },
                values: new object[] { 1009, "Errore rientro baia", 5, null });

            migrationBuilder.InsertData(
                table: "Errors",
                columns: new[] { "Code", "Description", "Issue", "Reason" },
                values: new object[] { 1008, "Errore rientro baia", 5, null });

            migrationBuilder.InsertData(
                table: "Errors",
                columns: new[] { "Code", "Description", "Issue", "Reason" },
                values: new object[] { 1007, "Errore rientro baia", 5, null });

            migrationBuilder.InsertData(
                table: "Errors",
                columns: new[] { "Code", "Description", "Issue", "Reason" },
                values: new object[] { 1006, "Errore rientro baia", 5, null });

            migrationBuilder.InsertData(
                table: "Errors",
                columns: new[] { "Code", "Description", "Issue", "Reason" },
                values: new object[] { 1005, "Errore rientro cassetto", 5, null });

            migrationBuilder.InsertData(
                table: "Errors",
                columns: new[] { "Code", "Description", "Issue", "Reason" },
                values: new object[] { 1004, "Errore salvataggio dati", 5, null });

            migrationBuilder.InsertData(
                table: "Errors",
                columns: new[] { "Code", "Description", "Issue", "Reason" },
                values: new object[] { 1003, "Errore inizializzazione dati", 5, null });

            migrationBuilder.InsertData(
                table: "Errors",
                columns: new[] { "Code", "Description", "Issue", "Reason" },
                values: new object[] { 1002, "Errore caricamento configurazione", 5, null });

            migrationBuilder.InsertData(
                table: "Errors",
                columns: new[] { "Code", "Description", "Issue", "Reason" },
                values: new object[] { 1017, "Errore posizionamento", 5, null });

            migrationBuilder.InsertData(
                table: "MachineStatistics",
                columns: new[] { "Id", "TotalBeltCycles", "TotalMovedTrays", "TotalMovedTraysInBay1", "TotalMovedTraysInBay2", "TotalMovedTraysInBay3", "TotalShutter1Cycles", "TotalShutter2Cycles", "TotalShutter3Cycles", "TotalVerticalAxisCycles", "TotalVerticalAxisKilometers" },
                values: new object[] { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0.0 });

            migrationBuilder.InsertData(
                table: "ErrorStatistics",
                columns: new[] { "Code", "TotalErrors" },
                values: new object[] { 1001, 11 });

            migrationBuilder.InsertData(
                table: "ErrorStatistics",
                columns: new[] { "Code", "TotalErrors" },
                values: new object[] { 1002, 7 });

            migrationBuilder.InsertData(
                table: "ErrorStatistics",
                columns: new[] { "Code", "TotalErrors" },
                values: new object[] { 1003, 5 });

            migrationBuilder.InsertData(
                table: "ErrorStatistics",
                columns: new[] { "Code", "TotalErrors" },
                values: new object[] { 1004, 3 });

            migrationBuilder.InsertData(
                table: "ErrorStatistics",
                columns: new[] { "Code", "TotalErrors" },
                values: new object[] { 1005, 2 });

            migrationBuilder.InsertData(
                table: "ErrorStatistics",
                columns: new[] { "Code", "TotalErrors" },
                values: new object[] { 1006, 1 });

            migrationBuilder.InsertData(
                table: "ErrorStatistics",
                columns: new[] { "Code", "TotalErrors" },
                values: new object[] { 1007, 1 });

            migrationBuilder.InsertData(
                table: "ErrorStatistics",
                columns: new[] { "Code", "TotalErrors" },
                values: new object[] { 1008, 1 });

            migrationBuilder.InsertData(
                table: "ErrorStatistics",
                columns: new[] { "Code", "TotalErrors" },
                values: new object[] { 1009, 1 });

            migrationBuilder.InsertData(
                table: "ErrorStatistics",
                columns: new[] { "Code", "TotalErrors" },
                values: new object[] { 1010, 1 });

            migrationBuilder.InsertData(
                table: "ErrorStatistics",
                columns: new[] { "Code", "TotalErrors" },
                values: new object[] { 1011, 1 });

            migrationBuilder.InsertData(
                table: "ErrorStatistics",
                columns: new[] { "Code", "TotalErrors" },
                values: new object[] { 1012, 1 });

            migrationBuilder.InsertData(
                table: "ErrorStatistics",
                columns: new[] { "Code", "TotalErrors" },
                values: new object[] { 1013, 1 });

            migrationBuilder.InsertData(
                table: "ErrorStatistics",
                columns: new[] { "Code", "TotalErrors" },
                values: new object[] { 1014, 1 });

            migrationBuilder.InsertData(
                table: "ErrorStatistics",
                columns: new[] { "Code", "TotalErrors" },
                values: new object[] { 1015, 1 });

            migrationBuilder.InsertData(
                table: "ErrorStatistics",
                columns: new[] { "Code", "TotalErrors" },
                values: new object[] { 1016, 0 });

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
                name: "ErrorStatistics");

            migrationBuilder.DropTable(
                name: "FreeBlocks");

            migrationBuilder.DropTable(
                name: "LogEntries");

            migrationBuilder.DropTable(
                name: "MachineStatistics");

            migrationBuilder.DropTable(
                name: "RuntimeValues");

            migrationBuilder.DropTable(
                name: "Errors");

            migrationBuilder.DropTable(
                name: "LoadingUnits");
        }
    }
}
