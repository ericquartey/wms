using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Ferretto.VW.MAS_DataLayer.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Cells",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Coord = table.Column<decimal>(nullable: false),
                    Priority = table.Column<int>(nullable: false),
                    Side = table.Column<int>(nullable: false),
                    Status = table.Column<int>(nullable: false),
                    WorkingStatus = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cells", x => x.Id);
                });

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
                name: "LoadingUnits",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Code = table.Column<string>(nullable: true),
                    CellId = table.Column<int>(nullable: true),
                    Description = table.Column<string>(nullable: true),
                    Height = table.Column<decimal>(nullable: false),
                    LoadingUnitId = table.Column<int>(nullable: false),
                    MaxNetWeight = table.Column<decimal>(nullable: false),
                    Tare = table.Column<decimal>(nullable: false),
                    Status = table.Column<long>(nullable: false),
                    GrossWeight = table.Column<decimal>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoadingUnits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LoadingUnits_Cells_CellId",
                        column: x => x.CellId,
                        principalTable: "Cells",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
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
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 1, 0m, 1, 0, 2, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 23, 0m, 23, 0, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 24, 0m, 24, 1, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 25, 0m, 25, 0, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 26, 0m, 26, 1, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 27, 0m, 27, 0, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 28, 0m, 28, 1, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 30, 0m, 30, 1, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 22, 0m, 22, 1, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 31, 0m, 31, 0, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 33, 0m, 33, 0, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 34, 0m, 34, 1, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 35, 0m, 35, 0, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 36, 0m, 36, 1, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 37, 0m, 37, 0, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 38, 0m, 38, 1, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 39, 0m, 39, 0, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 32, 0m, 32, 1, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 21, 0m, 21, 0, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 29, 0m, 29, 0, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 19, 0m, 19, 0, 2, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 20, 0m, 20, 1, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 3, 0m, 3, 0, 2, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 4, 0m, 4, 1, 2, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 5, 0m, 5, 0, 2, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 6, 0m, 6, 1, 2, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 7, 0m, 7, 0, 2, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 8, 0m, 8, 1, 2, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 9, 0m, 9, 0, 2, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 10, 0m, 10, 1, 2, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 2, 0m, 2, 1, 2, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 12, 0m, 12, 1, 2, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 13, 0m, 13, 0, 2, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 14, 0m, 14, 1, 2, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 15, 0m, 15, 0, 2, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 16, 0m, 16, 1, 2, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 17, 0m, 17, 0, 2, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 18, 0m, 18, 1, 2, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 11, 0m, 11, 0, 2, 0 });

            migrationBuilder.InsertData(
                table: "Errors",
                columns: new[] { "Code", "Description", "Issue", "Reason" },
                values: new object[] { 1017, "Errore posizionamento", 5, null });

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
                values: new object[] { 1005, "Errore rientro cassetto", 5, null });

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
                values: new object[] { 1001, "Errore database", 5, null });

            migrationBuilder.InsertData(
                table: "Errors",
                columns: new[] { "Code", "Description", "Issue", "Reason" },
                values: new object[] { 1009, "Errore rientro baia", 5, null });

            migrationBuilder.InsertData(
                table: "MachineStatistics",
                columns: new[] { "Id", "TotalBeltCycles", "TotalMovedTrays", "TotalMovedTraysInBay1", "TotalMovedTraysInBay2", "TotalMovedTraysInBay3", "TotalShutter1Cycles", "TotalShutter2Cycles", "TotalShutter3Cycles", "TotalVerticalAxisCycles", "TotalVerticalAxisKilometers" },
                values: new object[] { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0.0 });

            migrationBuilder.InsertData(
                table: "ErrorStatistics",
                columns: new[] { "Code", "TotalErrors" },
                values: new object[] { 1016, 0 });

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
                values: new object[] { 1008, 1 });

            migrationBuilder.InsertData(
                table: "ErrorStatistics",
                columns: new[] { "Code", "TotalErrors" },
                values: new object[] { 1015, 1 });

            migrationBuilder.InsertData(
                table: "LoadingUnits",
                columns: new[] { "Id", "CellId", "Code", "Description", "GrossWeight", "Height", "LoadingUnitId", "MaxNetWeight", "Status", "Tare" },
                values: new object[] { 19, 19, "LU#1.19", null, 0m, 100m, 0, 0m, 3L, 0m });

            migrationBuilder.InsertData(
                table: "LoadingUnits",
                columns: new[] { "Id", "CellId", "Code", "Description", "GrossWeight", "Height", "LoadingUnitId", "MaxNetWeight", "Status", "Tare" },
                values: new object[] { 18, 18, "LU#1.18", null, 0m, 100m, 0, 0m, 3L, 0m });

            migrationBuilder.InsertData(
                table: "LoadingUnits",
                columns: new[] { "Id", "CellId", "Code", "Description", "GrossWeight", "Height", "LoadingUnitId", "MaxNetWeight", "Status", "Tare" },
                values: new object[] { 16, 16, "LU#1.16", null, 0m, 100m, 0, 0m, 3L, 0m });

            migrationBuilder.InsertData(
                table: "LoadingUnits",
                columns: new[] { "Id", "CellId", "Code", "Description", "GrossWeight", "Height", "LoadingUnitId", "MaxNetWeight", "Status", "Tare" },
                values: new object[] { 2, 2, "LU#1.02", null, 0m, 100m, 0, 0m, 3L, 0m });

            migrationBuilder.InsertData(
                table: "LoadingUnits",
                columns: new[] { "Id", "CellId", "Code", "Description", "GrossWeight", "Height", "LoadingUnitId", "MaxNetWeight", "Status", "Tare" },
                values: new object[] { 3, 3, "LU#1.03", null, 0m, 100m, 0, 0m, 3L, 0m });

            migrationBuilder.InsertData(
                table: "LoadingUnits",
                columns: new[] { "Id", "CellId", "Code", "Description", "GrossWeight", "Height", "LoadingUnitId", "MaxNetWeight", "Status", "Tare" },
                values: new object[] { 4, 4, "LU#1.04", null, 0m, 100m, 0, 0m, 3L, 0m });

            migrationBuilder.InsertData(
                table: "LoadingUnits",
                columns: new[] { "Id", "CellId", "Code", "Description", "GrossWeight", "Height", "LoadingUnitId", "MaxNetWeight", "Status", "Tare" },
                values: new object[] { 5, 5, "LU#1.05", null, 0m, 100m, 0, 0m, 3L, 0m });

            migrationBuilder.InsertData(
                table: "LoadingUnits",
                columns: new[] { "Id", "CellId", "Code", "Description", "GrossWeight", "Height", "LoadingUnitId", "MaxNetWeight", "Status", "Tare" },
                values: new object[] { 6, 6, "LU#1.06", null, 0m, 100m, 0, 0m, 3L, 0m });

            migrationBuilder.InsertData(
                table: "LoadingUnits",
                columns: new[] { "Id", "CellId", "Code", "Description", "GrossWeight", "Height", "LoadingUnitId", "MaxNetWeight", "Status", "Tare" },
                values: new object[] { 7, 7, "LU#1.07", null, 0m, 100m, 0, 0m, 3L, 0m });

            migrationBuilder.InsertData(
                table: "LoadingUnits",
                columns: new[] { "Id", "CellId", "Code", "Description", "GrossWeight", "Height", "LoadingUnitId", "MaxNetWeight", "Status", "Tare" },
                values: new object[] { 17, 17, "LU#1.17", null, 0m, 100m, 0, 0m, 3L, 0m });

            migrationBuilder.InsertData(
                table: "LoadingUnits",
                columns: new[] { "Id", "CellId", "Code", "Description", "GrossWeight", "Height", "LoadingUnitId", "MaxNetWeight", "Status", "Tare" },
                values: new object[] { 8, 8, "LU#1.08", null, 0m, 100m, 0, 0m, 3L, 0m });

            migrationBuilder.InsertData(
                table: "LoadingUnits",
                columns: new[] { "Id", "CellId", "Code", "Description", "GrossWeight", "Height", "LoadingUnitId", "MaxNetWeight", "Status", "Tare" },
                values: new object[] { 10, 10, "LU#1.10", null, 0m, 100m, 0, 0m, 3L, 0m });

            migrationBuilder.InsertData(
                table: "LoadingUnits",
                columns: new[] { "Id", "CellId", "Code", "Description", "GrossWeight", "Height", "LoadingUnitId", "MaxNetWeight", "Status", "Tare" },
                values: new object[] { 11, 11, "LU#1.11", null, 0m, 100m, 0, 0m, 3L, 0m });

            migrationBuilder.InsertData(
                table: "LoadingUnits",
                columns: new[] { "Id", "CellId", "Code", "Description", "GrossWeight", "Height", "LoadingUnitId", "MaxNetWeight", "Status", "Tare" },
                values: new object[] { 12, 12, "LU#1.12", null, 0m, 100m, 0, 0m, 3L, 0m });

            migrationBuilder.InsertData(
                table: "LoadingUnits",
                columns: new[] { "Id", "CellId", "Code", "Description", "GrossWeight", "Height", "LoadingUnitId", "MaxNetWeight", "Status", "Tare" },
                values: new object[] { 13, 13, "LU#1.13", null, 0m, 100m, 0, 0m, 3L, 0m });

            migrationBuilder.InsertData(
                table: "LoadingUnits",
                columns: new[] { "Id", "CellId", "Code", "Description", "GrossWeight", "Height", "LoadingUnitId", "MaxNetWeight", "Status", "Tare" },
                values: new object[] { 14, 14, "LU#1.14", null, 0m, 100m, 0, 0m, 3L, 0m });

            migrationBuilder.InsertData(
                table: "LoadingUnits",
                columns: new[] { "Id", "CellId", "Code", "Description", "GrossWeight", "Height", "LoadingUnitId", "MaxNetWeight", "Status", "Tare" },
                values: new object[] { 15, 15, "LU#1.15", null, 0m, 100m, 0, 0m, 3L, 0m });

            migrationBuilder.InsertData(
                table: "LoadingUnits",
                columns: new[] { "Id", "CellId", "Code", "Description", "GrossWeight", "Height", "LoadingUnitId", "MaxNetWeight", "Status", "Tare" },
                values: new object[] { 9, 9, "LU#1.09", null, 0m, 100m, 0, 0m, 3L, 0m });

            migrationBuilder.InsertData(
                table: "LoadingUnits",
                columns: new[] { "Id", "CellId", "Code", "Description", "GrossWeight", "Height", "LoadingUnitId", "MaxNetWeight", "Status", "Tare" },
                values: new object[] { 1, 1, "LU#1.01", null, 0m, 100m, 0, 0m, 3L, 0m });

            migrationBuilder.CreateIndex(
                name: "IX_FreeBlocks_LoadingUnitId",
                table: "FreeBlocks",
                column: "LoadingUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_LoadingUnits_CellId",
                table: "LoadingUnits",
                column: "CellId",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
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

            migrationBuilder.DropTable(
                name: "Cells");
        }
    }
}
