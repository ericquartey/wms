using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Ferretto.VW.MAS.DataLayer.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CellPanels",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Side = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CellPanels", x => x.Id);
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
                name: "ElevatorStructuralProperties",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    BeltRigidity = table.Column<decimal>(nullable: false),
                    BeltSpacing = table.Column<decimal>(nullable: false),
                    HalfShaftLength = table.Column<decimal>(nullable: false),
                    Height = table.Column<decimal>(nullable: false),
                    MaximumLoadOnBoard = table.Column<decimal>(nullable: false),
                    PulleyDiameter = table.Column<decimal>(nullable: false),
                    ShaftDiameter = table.Column<decimal>(nullable: false),
                    ShaftElasticity = table.Column<decimal>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ElevatorStructuralProperties", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErrorDefinitions",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Code = table.Column<int>(nullable: false),
                    Description = table.Column<string>(nullable: false),
                    Reason = table.Column<string>(nullable: true),
                    Severity = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ErrorDefinitions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LogEntries",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
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
                    table.PrimaryKey("PK_LogEntries", x => x.Id);
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
                name: "MovementParameters",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Acceleration = table.Column<decimal>(nullable: false),
                    Deceleration = table.Column<decimal>(nullable: false),
                    Speed = table.Column<decimal>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MovementParameters", x => x.Id);
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
                name: "SetupStatus",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AllLoadingUnits = table.Column<bool>(nullable: false),
                    Bay1Check = table.Column<bool>(nullable: false),
                    Bay1FirstLoadingUnit = table.Column<bool>(nullable: false),
                    Bay1Laser = table.Column<bool>(nullable: false),
                    Bay1Shape = table.Column<bool>(nullable: false),
                    Bay1Shutter = table.Column<bool>(nullable: false),
                    Bay2Check = table.Column<bool>(nullable: false),
                    Bay2FirstLoadingUnit = table.Column<bool>(nullable: false),
                    Bay2Laser = table.Column<bool>(nullable: false),
                    Bay2Shape = table.Column<bool>(nullable: false),
                    Bay2Shutter = table.Column<bool>(nullable: false),
                    Bay3Check = table.Column<bool>(nullable: false),
                    Bay3FirstLoadingUnit = table.Column<bool>(nullable: false),
                    Bay3Laser = table.Column<bool>(nullable: false),
                    Bay3Shape = table.Column<bool>(nullable: false),
                    Bay3Shutter = table.Column<bool>(nullable: false),
                    BeltBurnishingCompleted = table.Column<bool>(nullable: false),
                    BeltBurnishingCompletedCycles = table.Column<int>(nullable: false),
                    BeltBurnishingRequiredCycles = table.Column<int>(nullable: false),
                    CellsHeightCheck = table.Column<bool>(nullable: false),
                    CompletedDate = table.Column<DateTime>(nullable: true),
                    HorizontalHoming = table.Column<bool>(nullable: false),
                    PanelsCheck = table.Column<bool>(nullable: false),
                    VerticalOffsetCalibration = table.Column<bool>(nullable: false),
                    VerticalResolution = table.Column<bool>(nullable: false),
                    WeightMeasurement = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SetupStatus", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TorqueCurrentMeasurementSessions",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    LoadedNetWeight = table.Column<decimal>(nullable: false),
                    LoadingUnitId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TorqueCurrentMeasurementSessions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AccessLevel = table.Column<int>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    PasswordHash = table.Column<string>(nullable: false),
                    PasswordSalt = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Cells",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PanelId = table.Column<int>(nullable: false),
                    Position = table.Column<decimal>(nullable: false),
                    Priority = table.Column<int>(nullable: false),
                    Status = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cells", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Cells_CellPanels_PanelId",
                        column: x => x.PanelId,
                        principalTable: "CellPanels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Elevators",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    LoadingUnitOnBoard = table.Column<int>(nullable: true),
                    StructuralPropertiesId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Elevators", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Elevators_ElevatorStructuralProperties_StructuralPropertiesId",
                        column: x => x.StructuralPropertiesId,
                        principalTable: "ElevatorStructuralProperties",
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
                        name: "FK_ErrorStatistics_ErrorDefinitions_Code",
                        column: x => x.Code,
                        principalTable: "ErrorDefinitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TorqueCurrentSamples",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    MeasurementSessionId = table.Column<int>(nullable: false),
                    RequestTimeStamp = table.Column<DateTime>(nullable: false),
                    TimeStamp = table.Column<DateTime>(nullable: false),
                    Value = table.Column<decimal>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TorqueCurrentSamples", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TorqueCurrentSamples_TorqueCurrentMeasurementSessions_MeasurementSessionId",
                        column: x => x.MeasurementSessionId,
                        principalTable: "TorqueCurrentMeasurementSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
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
                    IsIntoMachine = table.Column<bool>(nullable: false),
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
                name: "ElevatorAxes",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    EmptyLoadId = table.Column<int>(nullable: true),
                    LowerBound = table.Column<decimal>(nullable: false),
                    MaximumLoadId = table.Column<int>(nullable: true),
                    Offset = table.Column<decimal>(nullable: false),
                    Orientation = table.Column<int>(nullable: false),
                    Resolution = table.Column<decimal>(nullable: false),
                    UpperBound = table.Column<decimal>(nullable: false),
                    ElevatorId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ElevatorAxes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ElevatorAxes_Elevators_ElevatorId",
                        column: x => x.ElevatorId,
                        principalTable: "Elevators",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ElevatorAxes_MovementParameters_EmptyLoadId",
                        column: x => x.EmptyLoadId,
                        principalTable: "MovementParameters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ElevatorAxes_MovementParameters_MaximumLoadId",
                        column: x => x.MaximumLoadId,
                        principalTable: "MovementParameters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Bays",
                columns: table => new
                {
                    Number = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CurrentMissionId = table.Column<int>(nullable: true),
                    CurrentMissionOperationId = table.Column<int>(nullable: true),
                    ExternalId = table.Column<int>(nullable: false),
                    IpAddress = table.Column<string>(nullable: true),
                    IsActive = table.Column<bool>(nullable: false),
                    LoadingUnitId = table.Column<int>(nullable: true),
                    Type = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bays", x => x.Number);
                    table.ForeignKey(
                        name: "FK_Bays_LoadingUnits_LoadingUnitId",
                        column: x => x.LoadingUnitId,
                        principalTable: "LoadingUnits",
                        principalColumn: "Id",
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
                    LoadingUnitId = table.Column<int>(nullable: false),
                    Position = table.Column<decimal>(nullable: false),
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
                table: "ErrorDefinitions",
                columns: new[] { "Id", "Code", "Description", "Reason", "Severity" },
                values: new object[] { 100032, 100032, "Cassetto non caricato completamente", "Il cassetto potrebbe essersi incastrato.", 0 });

            migrationBuilder.InsertData(
                table: "MachineStatistics",
                columns: new[] { "Id", "TotalAutomaticTime", "TotalBeltCycles", "TotalMissionTime", "TotalMovedTrays", "TotalMovedTraysInBay1", "TotalMovedTraysInBay2", "TotalMovedTraysInBay3", "TotalPowerOnTime", "TotalShutter1Cycles", "TotalShutter2Cycles", "TotalShutter3Cycles", "TotalVerticalAxisCycles", "TotalVerticalAxisKilometers", "WeightCapacityPercentage" },
                values: new object[] { 1, new TimeSpan(130, 0, 0, 0, 0), 12352, new TimeSpan(30, 0, 0, 0, 0), 534, 123, 456, 789, new TimeSpan(190, 0, 0, 0, 0), 321, 654, 987, 5232, 34.0, 60.0 });

            migrationBuilder.InsertData(
                table: "ServicingInfo",
                columns: new[] { "Id", "InstallationDate", "LastServiceDate", "NextServiceDate", "ServiceStatus" },
                values: new object[] { 1, new DateTime(2016, 11, 19, 16, 51, 38, 827, DateTimeKind.Local).AddTicks(5976), null, null, 86 });

            migrationBuilder.InsertData(
                table: "SetupStatus",
                columns: new[] { "Id", "AllLoadingUnits", "Bay1Check", "Bay1FirstLoadingUnit", "Bay1Laser", "Bay1Shape", "Bay1Shutter", "Bay2Check", "Bay2FirstLoadingUnit", "Bay2Laser", "Bay2Shape", "Bay2Shutter", "Bay3Check", "Bay3FirstLoadingUnit", "Bay3Laser", "Bay3Shape", "Bay3Shutter", "BeltBurnishingCompleted", "BeltBurnishingCompletedCycles", "BeltBurnishingRequiredCycles", "CellsHeightCheck", "CompletedDate", "HorizontalHoming", "PanelsCheck", "VerticalOffsetCalibration", "VerticalResolution", "WeightMeasurement" },
                values: new object[] { 1, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, 0, 0, false, null, false, false, false, false, false });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "AccessLevel", "Name", "PasswordHash", "PasswordSalt" },
                values: new object[] { -1, 0, "installer", "DsWpG30CTZweMD4Q+LlgzrsGOWM/jx6enmP8O7RIrvU=", "2xw+hMIYBtLCoUqQGXSL0A==" });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "AccessLevel", "Name", "PasswordHash", "PasswordSalt" },
                values: new object[] { -2, 2, "operator", "e1IrRSpcUNLIQAmdtSzQqrKT4DLcMaYMh662pgMh2xY=", "iB+IdMnlzvXvitHWJff38A==" });

            migrationBuilder.InsertData(
                table: "ErrorStatistics",
                columns: new[] { "Code", "TotalErrors" },
                values: new object[] { 100032, 0 });

            migrationBuilder.CreateIndex(
                name: "IX_Bays_IpAddress",
                table: "Bays",
                column: "IpAddress",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Bays_LoadingUnitId",
                table: "Bays",
                column: "LoadingUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_Cells_PanelId",
                table: "Cells",
                column: "PanelId");

            migrationBuilder.CreateIndex(
                name: "IX_ElevatorAxes_ElevatorId",
                table: "ElevatorAxes",
                column: "ElevatorId");

            migrationBuilder.CreateIndex(
                name: "IX_ElevatorAxes_EmptyLoadId",
                table: "ElevatorAxes",
                column: "EmptyLoadId");

            migrationBuilder.CreateIndex(
                name: "IX_ElevatorAxes_MaximumLoadId",
                table: "ElevatorAxes",
                column: "MaximumLoadId");

            migrationBuilder.CreateIndex(
                name: "IX_Elevators_StructuralPropertiesId",
                table: "Elevators",
                column: "StructuralPropertiesId");

            migrationBuilder.CreateIndex(
                name: "IX_ErrorDefinitions_Code",
                table: "ErrorDefinitions",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Errors_Code",
                table: "Errors",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FreeBlocks_LoadingUnitId",
                table: "FreeBlocks",
                column: "LoadingUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_LoadingUnits_CellId",
                table: "LoadingUnits",
                column: "CellId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LoadingUnits_Code",
                table: "LoadingUnits",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TorqueCurrentSamples_MeasurementSessionId",
                table: "TorqueCurrentSamples",
                column: "MeasurementSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Name",
                table: "Users",
                column: "Name",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Bays");

            migrationBuilder.DropTable(
                name: "ConfigurationValues");

            migrationBuilder.DropTable(
                name: "ElevatorAxes");

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
                name: "SetupStatus");

            migrationBuilder.DropTable(
                name: "TorqueCurrentSamples");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Elevators");

            migrationBuilder.DropTable(
                name: "MovementParameters");

            migrationBuilder.DropTable(
                name: "ErrorDefinitions");

            migrationBuilder.DropTable(
                name: "LoadingUnits");

            migrationBuilder.DropTable(
                name: "TorqueCurrentMeasurementSessions");

            migrationBuilder.DropTable(
                name: "ElevatorStructuralProperties");

            migrationBuilder.DropTable(
                name: "Cells");

            migrationBuilder.DropTable(
                name: "CellPanels");
        }
    }
}
