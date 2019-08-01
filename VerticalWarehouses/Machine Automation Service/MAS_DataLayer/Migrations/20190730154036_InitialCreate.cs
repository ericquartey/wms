using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Ferretto.VW.MAS.DataLayer.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Bays",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CurrentMissionId = table.Column<int>(nullable: true),
                    CurrentMissionOperationId = table.Column<int>(nullable: true),
                    ExternalId = table.Column<int>(nullable: false),
                    IpAddress = table.Column<string>(nullable: true),
                    IsActive = table.Column<bool>(nullable: false),
                    Type = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bays", x => x.Id);
                });

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
                name: "ErrorDefinitions",
                columns: table => new
                {
                    Code = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Description = table.Column<string>(nullable: false),
                    Reason = table.Column<string>(nullable: true),
                    Severity = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ErrorDefinitions", x => x.Code);
                });

            migrationBuilder.CreateTable(
                name: "LogEntries",
                columns: table => new
                {
                    LogEntryID = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Data = table.Column<string>(nullable: true),
                    Description = table.Column<string>(nullable: true),
                    Destination = table.Column<string>(nullable: true),
                    ErrorLevel = table.Column<string>(nullable: true),
                    Exception = table.Column<string>(nullable: true),
                    Level = table.Column<string>(nullable: true),
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
                    TotalMovedTrays = table.Column<int>(nullable: false),
                    TotalPowerOnTime = table.Column<TimeSpan>(nullable: false),
                    TotalAutomaticTime = table.Column<TimeSpan>(nullable: false),
                    TotalMissionTime = table.Column<TimeSpan>(nullable: false),
                    WeightCapacityPercentage = table.Column<double>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MachineStatistics", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ServicingInfo",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    InstallationDate = table.Column<DateTime>(nullable: false),
                    LastServiceDate = table.Column<DateTime>(nullable: true),
                    NextServiceDate = table.Column<DateTime>(nullable: true),
                    ServiceStatus = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServicingInfo", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Name = table.Column<string>(nullable: false),
                    AccessLevel = table.Column<int>(nullable: false),
                    PasswordHash = table.Column<string>(nullable: false),
                    PasswordSalt = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Name);
                });

            migrationBuilder.CreateTable(
                name: "LoadingUnits",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CellId = table.Column<int>(nullable: true),
                    Code = table.Column<string>(nullable: true),
                    Description = table.Column<string>(nullable: true),
                    GrossWeight = table.Column<decimal>(nullable: false),
                    Height = table.Column<decimal>(nullable: false),
                    MaxNetWeight = table.Column<decimal>(nullable: false),
                    MissionsCount = table.Column<int>(nullable: false),
                    Status = table.Column<long>(nullable: false),
                    Tare = table.Column<decimal>(nullable: false)
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
                name: "Errors",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Code = table.Column<int>(nullable: false),
                    OccurrenceDate = table.Column<DateTime>(nullable: false),
                    ResolutionDate = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Errors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Errors_ErrorDefinitions_Code",
                        column: x => x.Code,
                        principalTable: "ErrorDefinitions",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Cascade);
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
                        name: "FK_ErrorStatistics_ErrorDefinitions_Code",
                        column: x => x.Code,
                        principalTable: "ErrorDefinitions",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FreeBlocks",
                columns: table => new
                {
                    FreeBlockId = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    BlockSize = table.Column<int>(nullable: false),
                    BookedCellsNumber = table.Column<int>(nullable: false),
                    Coord = table.Column<decimal>(nullable: false),
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
                values: new object[] { 1, 268m, 1, 0, 2, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 259, 6718m, 259, 0, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 258, 6718m, 258, 1, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 257, 6668m, 257, 0, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 256, 6668m, 256, 1, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 255, 6618m, 255, 0, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 254, 6618m, 254, 1, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 253, 6568m, 253, 0, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 252, 6568m, 252, 1, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 251, 6518m, 251, 0, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 250, 6518m, 250, 1, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 249, 6468m, 249, 0, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 248, 6468m, 248, 1, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 247, 6418m, 247, 0, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 246, 6418m, 246, 1, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 245, 6368m, 245, 0, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 244, 6368m, 244, 1, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 243, 6318m, 243, 0, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 242, 6318m, 242, 1, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 241, 6268m, 241, 0, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 260, 6768m, 260, 1, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 261, 6768m, 261, 0, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 262, 6818m, 262, 1, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 263, 6818m, 263, 0, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 283, 7318m, 283, 0, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 282, 7318m, 282, 1, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 281, 7268m, 281, 0, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 280, 7268m, 280, 1, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 279, 7218m, 279, 0, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 278, 7218m, 278, 1, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 277, 7168m, 277, 0, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 276, 7168m, 276, 1, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 275, 7118m, 275, 0, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 240, 6268m, 240, 1, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 274, 7118m, 274, 1, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 272, 7068m, 272, 1, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 271, 7018m, 271, 0, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 270, 7018m, 270, 1, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 269, 6968m, 269, 0, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 268, 6968m, 268, 1, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 267, 6918m, 267, 0, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 266, 6918m, 266, 1, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 265, 6868m, 265, 0, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 264, 6868m, 264, 1, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 273, 7068m, 273, 0, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 239, 6218m, 239, 0, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 238, 6218m, 238, 1, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 237, 6168m, 237, 0, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 212, 5568m, 212, 1, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 211, 5518m, 211, 0, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 210, 5518m, 210, 1, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 209, 5468m, 209, 0, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 208, 5468m, 208, 1, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 207, 5418m, 207, 0, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 206, 5418m, 206, 1, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 205, 5368m, 205, 0, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 204, 5368m, 204, 1, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 213, 5568m, 213, 0, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 203, 5318m, 203, 0, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 201, 5268m, 201, 0, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 200, 5268m, 200, 1, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 199, 5218m, 199, 0, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 198, 5218m, 198, 1, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 197, 5168m, 197, 0, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 196, 5168m, 196, 1, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 195, 5118m, 195, 0, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 194, 5118m, 194, 1, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 193, 5068m, 193, 0, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 202, 5318m, 202, 1, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 284, 7368m, 284, 1, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 214, 5618m, 214, 1, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 216, 5668m, 216, 1, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 236, 6168m, 236, 1, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 235, 6118m, 235, 0, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 234, 6118m, 234, 1, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 233, 6068m, 233, 0, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 232, 6068m, 232, 1, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 231, 6018m, 231, 0, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 230, 6018m, 230, 1, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 229, 5968m, 229, 0, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 228, 5968m, 228, 1, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 215, 5618m, 215, 0, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 227, 5918m, 227, 0, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 225, 5868m, 225, 0, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 224, 5868m, 224, 1, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 223, 5818m, 223, 0, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 222, 5818m, 222, 1, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 221, 5768m, 221, 0, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 220, 5768m, 220, 1, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 219, 5718m, 219, 0, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 218, 5718m, 218, 1, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 217, 5668m, 217, 0, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 226, 5918m, 226, 1, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 191, 5018m, 191, 0, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 285, 7368m, 285, 0, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 287, 7418m, 287, 0, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 354, 9118m, 354, 1, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 353, 9068m, 353, 0, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 352, 9068m, 352, 1, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 351, 9018m, 351, 0, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 350, 9018m, 350, 1, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 349, 8968m, 349, 0, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 348, 8968m, 348, 1, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 347, 8918m, 347, 0, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 346, 8918m, 346, 1, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 345, 8868m, 345, 0, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 344, 8868m, 344, 1, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 343, 8818m, 343, 0, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 342, 8818m, 342, 1, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 341, 8768m, 341, 0, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 340, 8768m, 340, 1, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 339, 8718m, 339, 0, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 338, 8718m, 338, 1, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 337, 8668m, 337, 0, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 336, 8668m, 336, 1, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 355, 9118m, 355, 0, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 356, 9168m, 356, 1, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 357, 9168m, 357, 0, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 358, 9218m, 358, 1, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 378, 9718m, 378, 1, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 377, 9668m, 377, 0, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 376, 9668m, 376, 1, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 375, 9618m, 375, 0, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 374, 9618m, 374, 1, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 373, 9568m, 373, 0, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 372, 9568m, 372, 1, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 371, 9518m, 371, 0, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 370, 9518m, 370, 1, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 335, 8618m, 335, 0, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 369, 9468m, 369, 0, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 367, 9418m, 367, 0, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 366, 9418m, 366, 1, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 365, 9368m, 365, 0, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 364, 9368m, 364, 1, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 363, 9318m, 363, 0, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 362, 9318m, 362, 1, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 361, 9268m, 361, 0, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 360, 9268m, 360, 1, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 359, 9218m, 359, 0, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 368, 9468m, 368, 1, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 334, 8618m, 334, 1, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 333, 8568m, 333, 0, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 332, 8568m, 332, 1, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 307, 7918m, 307, 0, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 306, 7918m, 306, 1, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 305, 7868m, 305, 0, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 304, 7868m, 304, 1, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 303, 7818m, 303, 0, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 302, 7818m, 302, 1, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 301, 7768m, 301, 0, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 300, 7768m, 300, 1, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 299, 7718m, 299, 0, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 308, 7968m, 308, 1, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 298, 7718m, 298, 1, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 296, 7668m, 296, 1, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 295, 7618m, 295, 0, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 294, 7618m, 294, 1, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 293, 7568m, 293, 0, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 292, 7568m, 292, 1, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 291, 7518m, 291, 0, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 290, 7518m, 290, 1, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 289, 7468m, 289, 0, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 288, 7468m, 288, 1, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 297, 7668m, 297, 0, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 286, 7418m, 286, 1, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 309, 7968m, 309, 0, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 311, 8018m, 311, 0, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 331, 8518m, 331, 0, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 330, 8518m, 330, 1, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 329, 8468m, 329, 0, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 328, 8468m, 328, 1, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 327, 8418m, 327, 0, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 326, 8418m, 326, 1, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 325, 8368m, 325, 0, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 324, 8368m, 324, 1, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 323, 8318m, 323, 0, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 310, 8018m, 310, 1, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 322, 8318m, 322, 1, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 320, 8268m, 320, 1, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 319, 8218m, 319, 0, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 318, 8218m, 318, 1, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 317, 8168m, 317, 0, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 316, 8168m, 316, 1, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 315, 8118m, 315, 0, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 314, 8118m, 314, 1, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 313, 8068m, 313, 0, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 312, 8068m, 312, 1, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 321, 8268m, 321, 0, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 190, 5018m, 190, 1, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 192, 5068m, 192, 1, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 188, 4968m, 188, 1, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 68, 1968m, 68, 1, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 67, 1918m, 67, 0, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 66, 1918m, 66, 1, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 65, 1868m, 65, 0, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 64, 1868m, 64, 1, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 63, 1818m, 63, 0, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 62, 1818m, 62, 1, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 61, 1768m, 61, 0, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 60, 1768m, 60, 1, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 59, 1718m, 59, 0, 1, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 58, 1718m, 58, 1, 1, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 57, 1668m, 57, 0, 1, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 56, 1668m, 56, 1, 1, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 55, 1618m, 55, 0, 1, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 54, 1618m, 54, 1, 1, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 53, 1568m, 53, 0, 1, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 52, 1568m, 52, 1, 1, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 51, 1518m, 51, 0, 1, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 50, 1518m, 50, 1, 1, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 69, 1968m, 69, 0, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 70, 2018m, 70, 1, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 71, 2018m, 71, 0, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 72, 2068m, 72, 1, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 92, 2568m, 92, 1, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 91, 2518m, 91, 0, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 90, 2518m, 90, 1, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 89, 2468m, 89, 0, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 88, 2468m, 88, 1, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 87, 2418m, 87, 0, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 86, 2418m, 86, 1, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 85, 2368m, 85, 0, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 84, 2368m, 84, 1, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 49, 1468m, 49, 0, 3, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 83, 2318m, 83, 0, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 81, 2268m, 81, 0, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 80, 2268m, 80, 1, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 79, 2218m, 79, 0, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 78, 2218m, 78, 1, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 77, 2168m, 77, 0, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 76, 2168m, 76, 1, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 75, 2118m, 75, 0, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 74, 2118m, 74, 1, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 73, 2068m, 73, 0, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 82, 2318m, 82, 1, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 48, 1468m, 48, 1, 3, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 47, 1418m, 47, 0, 3, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 46, 1418m, 46, 1, 3, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 21, 768m, 21, 0, 2, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 20, 768m, 20, 1, 2, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 19, 718m, 19, 0, 2, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 18, 718m, 18, 1, 2, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 17, 668m, 17, 0, 2, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 16, 668m, 16, 1, 2, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 15, 618m, 15, 0, 2, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 14, 618m, 14, 1, 2, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 13, 568m, 13, 0, 2, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 22, 818m, 22, 1, 2, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 12, 568m, 12, 1, 2, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 10, 518m, 10, 1, 2, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 9, 468m, 9, 0, 2, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 8, 468m, 8, 1, 2, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 7, 418m, 7, 0, 2, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 6, 418m, 6, 1, 2, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 5, 368m, 5, 0, 2, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 4, 368m, 4, 1, 2, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 3, 318m, 3, 0, 2, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 2, 318m, 2, 1, 2, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 11, 518m, 11, 0, 2, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 189, 4968m, 189, 0, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 23, 818m, 23, 0, 2, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 25, 868m, 25, 0, 2, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 45, 1368m, 45, 0, 3, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 44, 1368m, 44, 1, 3, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 43, 1318m, 43, 0, 3, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 42, 1318m, 42, 1, 3, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 41, 1268m, 41, 0, 3, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 40, 1268m, 40, 1, 3, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 39, 1218m, 39, 0, 2, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 38, 1218m, 38, 1, 2, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 37, 1168m, 37, 0, 2, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 24, 868m, 24, 1, 2, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 36, 1168m, 36, 1, 2, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 34, 1118m, 34, 1, 2, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 33, 1068m, 33, 0, 2, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 32, 1068m, 32, 1, 2, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 31, 1018m, 31, 0, 2, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 30, 1018m, 30, 1, 2, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 29, 968m, 29, 0, 2, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 28, 968m, 28, 1, 2, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 27, 918m, 27, 0, 2, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 26, 918m, 26, 1, 2, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 35, 1118m, 35, 0, 2, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 94, 2618m, 94, 1, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 93, 2568m, 93, 0, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 96, 2668m, 96, 1, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 163, 4318m, 163, 0, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 162, 4318m, 162, 1, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 161, 4268m, 161, 0, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 160, 4268m, 160, 1, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 159, 4218m, 159, 0, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 158, 4218m, 158, 1, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 157, 4168m, 157, 0, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 156, 4168m, 156, 1, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 155, 4118m, 155, 0, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 154, 4118m, 154, 1, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 153, 4068m, 153, 0, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 152, 4068m, 152, 1, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 151, 4018m, 151, 0, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 150, 4018m, 150, 1, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 149, 3968m, 149, 0, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 148, 3968m, 148, 1, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 147, 3918m, 147, 0, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 146, 3918m, 146, 1, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 145, 3868m, 145, 0, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 164, 4368m, 164, 1, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 165, 4368m, 165, 0, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 166, 4418m, 166, 1, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 167, 4418m, 167, 0, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 187, 4918m, 187, 0, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 186, 4918m, 186, 1, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 185, 4868m, 185, 0, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 184, 4868m, 184, 1, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 183, 4818m, 183, 0, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 182, 4818m, 182, 1, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 181, 4768m, 181, 0, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 180, 4768m, 180, 1, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 179, 4718m, 179, 0, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 95, 2618m, 95, 0, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 178, 4718m, 178, 1, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 176, 4668m, 176, 1, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 175, 4618m, 175, 0, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 174, 4618m, 174, 1, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 173, 4568m, 173, 0, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 172, 4568m, 172, 1, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 171, 4518m, 171, 0, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 170, 4518m, 170, 1, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 169, 4468m, 169, 0, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 168, 4468m, 168, 1, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 177, 4668m, 177, 0, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 143, 3818m, 143, 0, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 144, 3868m, 144, 1, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 141, 3768m, 141, 0, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 142, 3818m, 142, 1, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 115, 3118m, 115, 0, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 114, 3118m, 114, 1, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 113, 3068m, 113, 0, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 112, 3068m, 112, 1, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 111, 3018m, 111, 0, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 110, 3018m, 110, 1, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 109, 2968m, 109, 0, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 108, 2968m, 108, 1, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 117, 3168m, 117, 0, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 107, 2918m, 107, 0, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 105, 2868m, 105, 0, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 104, 2868m, 104, 1, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 103, 2818m, 103, 0, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 102, 2818m, 102, 1, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 101, 2768m, 101, 0, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 100, 2768m, 100, 1, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 99, 2718m, 99, 0, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 98, 2718m, 98, 1, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 97, 2668m, 97, 0, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 106, 2918m, 106, 1, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 118, 3218m, 118, 1, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 116, 3168m, 116, 1, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 120, 3268m, 120, 1, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 140, 3768m, 140, 1, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 139, 3718m, 139, 0, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 138, 3718m, 138, 1, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 137, 3668m, 137, 0, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 136, 3668m, 136, 1, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 135, 3618m, 135, 0, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 134, 3618m, 134, 1, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 119, 3218m, 119, 0, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 132, 3568m, 132, 1, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 131, 3518m, 131, 0, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 133, 3568m, 133, 0, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 129, 3468m, 129, 0, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 128, 3468m, 128, 1, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 127, 3418m, 127, 0, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 126, 3418m, 126, 1, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 125, 3368m, 125, 0, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 124, 3368m, 124, 1, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 123, 3318m, 123, 0, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 122, 3318m, 122, 1, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 121, 3268m, 121, 0, 0, 0 });

            migrationBuilder.InsertData(
                table: "Cells",
                columns: new[] { "Id", "Coord", "Priority", "Side", "Status", "WorkingStatus" },
                values: new object[] { 130, 3518m, 130, 1, 0, 0 });

            migrationBuilder.InsertData(
                table: "ErrorDefinitions",
                columns: new[] { "Code", "Description", "Reason", "Severity" },
                values: new object[] { 100032, "Il cassetto potrebbe essersi incastrato.", null, 0 });

            migrationBuilder.InsertData(
                table: "MachineStatistics",
                columns: new[] { "Id", "TotalAutomaticTime", "TotalBeltCycles", "TotalMissionTime", "TotalMovedTrays", "TotalMovedTraysInBay1", "TotalMovedTraysInBay2", "TotalMovedTraysInBay3", "TotalPowerOnTime", "TotalShutter1Cycles", "TotalShutter2Cycles", "TotalShutter3Cycles", "TotalVerticalAxisCycles", "TotalVerticalAxisKilometers", "WeightCapacityPercentage" },
                values: new object[] { 1, new TimeSpan(130, 0, 0, 0, 0), 12352, new TimeSpan(30, 0, 0, 0, 0), 534, 123, 456, 789, new TimeSpan(190, 0, 0, 0, 0), 321, 654, 987, 5232, 34.0, 60.0 });

            migrationBuilder.InsertData(
                table: "ServicingInfo",
                columns: new[] { "Id", "InstallationDate", "LastServiceDate", "NextServiceDate", "ServiceStatus" },
                values: new object[] { 1, new DateTime(2016, 9, 30, 17, 40, 35, 868, DateTimeKind.Local).AddTicks(5903), null, null, 86 });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Name", "AccessLevel", "PasswordHash", "PasswordSalt" },
                values: new object[] { "installer", 0, "DsWpG30CTZweMD4Q+LlgzrsGOWM/jx6enmP8O7RIrvU=", "2xw+hMIYBtLCoUqQGXSL0A==" });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Name", "AccessLevel", "PasswordHash", "PasswordSalt" },
                values: new object[] { "operator", 2, "e1IrRSpcUNLIQAmdtSzQqrKT4DLcMaYMh662pgMh2xY=", "iB+IdMnlzvXvitHWJff38A==" });

            migrationBuilder.InsertData(
                table: "ErrorStatistics",
                columns: new[] { "Code", "TotalErrors" },
                values: new object[] { 100032, 180 });

            migrationBuilder.InsertData(
                table: "LoadingUnits",
                columns: new[] { "Id", "CellId", "Code", "Description", "GrossWeight", "Height", "MaxNetWeight", "MissionsCount", "Status", "Tare" },
                values: new object[] { 1, 1, "01001", null, 334m, 166m, 500m, 6, 3L, 50m });

            migrationBuilder.InsertData(
                table: "LoadingUnits",
                columns: new[] { "Id", "CellId", "Code", "Description", "GrossWeight", "Height", "MaxNetWeight", "MissionsCount", "Status", "Tare" },
                values: new object[] { 2, 2, "01002", null, 392m, 62m, 500m, 2, 3L, 50m });

            migrationBuilder.InsertData(
                table: "LoadingUnits",
                columns: new[] { "Id", "CellId", "Code", "Description", "GrossWeight", "Height", "MaxNetWeight", "MissionsCount", "Status", "Tare" },
                values: new object[] { 3, 3, "01003", null, 372m, 54m, 750m, 22, 3L, 65m });

            migrationBuilder.InsertData(
                table: "LoadingUnits",
                columns: new[] { "Id", "CellId", "Code", "Description", "GrossWeight", "Height", "MaxNetWeight", "MissionsCount", "Status", "Tare" },
                values: new object[] { 4, 4, "01004", null, 218m, 241m, 500m, 42, 3L, 50m });

            migrationBuilder.InsertData(
                table: "LoadingUnits",
                columns: new[] { "Id", "CellId", "Code", "Description", "GrossWeight", "Height", "MaxNetWeight", "MissionsCount", "Status", "Tare" },
                values: new object[] { 5, 5, "01005", null, 296m, 56m, 500m, 8, 3L, 50m });

            migrationBuilder.InsertData(
                table: "LoadingUnits",
                columns: new[] { "Id", "CellId", "Code", "Description", "GrossWeight", "Height", "MaxNetWeight", "MissionsCount", "Status", "Tare" },
                values: new object[] { 6, 6, "01006", null, 395m, 212m, 500m, 5, 3L, 50m });

            migrationBuilder.InsertData(
                table: "LoadingUnits",
                columns: new[] { "Id", "CellId", "Code", "Description", "GrossWeight", "Height", "MaxNetWeight", "MissionsCount", "Status", "Tare" },
                values: new object[] { 7, 7, "01007", null, 374m, 329m, 500m, 7, 3L, 50m });

            migrationBuilder.InsertData(
                table: "LoadingUnits",
                columns: new[] { "Id", "CellId", "Code", "Description", "GrossWeight", "Height", "MaxNetWeight", "MissionsCount", "Status", "Tare" },
                values: new object[] { 8, 8, "01008", null, 285m, 228m, 500m, 41, 3L, 50m });

            migrationBuilder.InsertData(
                table: "LoadingUnits",
                columns: new[] { "Id", "CellId", "Code", "Description", "GrossWeight", "Height", "MaxNetWeight", "MissionsCount", "Status", "Tare" },
                values: new object[] { 9, 9, "01009", null, 394m, 257m, 500m, 19, 3L, 50m });

            migrationBuilder.InsertData(
                table: "LoadingUnits",
                columns: new[] { "Id", "CellId", "Code", "Description", "GrossWeight", "Height", "MaxNetWeight", "MissionsCount", "Status", "Tare" },
                values: new object[] { 10, 10, "01010", null, 298m, 71m, 500m, 3, 3L, 50m });

            migrationBuilder.InsertData(
                table: "LoadingUnits",
                columns: new[] { "Id", "CellId", "Code", "Description", "GrossWeight", "Height", "MaxNetWeight", "MissionsCount", "Status", "Tare" },
                values: new object[] { 11, 11, "01011", null, 296m, 171m, 500m, 23, 3L, 50m });

            migrationBuilder.InsertData(
                table: "LoadingUnits",
                columns: new[] { "Id", "CellId", "Code", "Description", "GrossWeight", "Height", "MaxNetWeight", "MissionsCount", "Status", "Tare" },
                values: new object[] { 12, 12, "01012", null, 396m, 303m, 750m, 7, 3L, 65m });

            migrationBuilder.InsertData(
                table: "LoadingUnits",
                columns: new[] { "Id", "CellId", "Code", "Description", "GrossWeight", "Height", "MaxNetWeight", "MissionsCount", "Status", "Tare" },
                values: new object[] { 13, 13, "01013", null, 265m, 132m, 750m, 43, 3L, 65m });

            migrationBuilder.InsertData(
                table: "LoadingUnits",
                columns: new[] { "Id", "CellId", "Code", "Description", "GrossWeight", "Height", "MaxNetWeight", "MissionsCount", "Status", "Tare" },
                values: new object[] { 14, 14, "01014", null, 346m, 186m, 500m, 13, 3L, 50m });

            migrationBuilder.InsertData(
                table: "LoadingUnits",
                columns: new[] { "Id", "CellId", "Code", "Description", "GrossWeight", "Height", "MaxNetWeight", "MissionsCount", "Status", "Tare" },
                values: new object[] { 15, 15, "01015", null, 337m, 149m, 500m, 25, 3L, 50m });

            migrationBuilder.CreateIndex(
                name: "IX_Bays_IpAddress",
                table: "Bays",
                column: "IpAddress",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Errors_Code",
                table: "Errors",
                column: "Code");

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
                name: "Bays");

            migrationBuilder.DropTable(
                name: "ConfigurationValues");

            migrationBuilder.DropTable(
                name: "Errors");

            migrationBuilder.DropTable(
                name: "ErrorStatistics");

            migrationBuilder.DropTable(
                name: "FreeBlocks");

            migrationBuilder.DropTable(
                name: "LogEntries");

            migrationBuilder.DropTable(
                name: "MachineStatistics");

            migrationBuilder.DropTable(
                name: "ServicingInfo");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "ErrorDefinitions");

            migrationBuilder.DropTable(
                name: "LoadingUnits");

            migrationBuilder.DropTable(
                name: "Cells");
        }
    }
}
