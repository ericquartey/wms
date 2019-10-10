﻿using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Ferretto.VW.MAS.DataLayer.Migrations
{
    public partial class initialcreation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Carousels",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ElevatorDistance = table.Column<double>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Carousels", x => x.Id);
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
                    BeltRigidity = table.Column<int>(nullable: false),
                    BeltSpacing = table.Column<double>(nullable: false),
                    HalfShaftLength = table.Column<double>(nullable: false),
                    MaximumLoadOnBoard = table.Column<double>(nullable: false),
                    PulleyDiameter = table.Column<double>(nullable: false),
                    ShaftDiameter = table.Column<double>(nullable: false),
                    ShaftElasticity = table.Column<double>(nullable: false)
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
                name: "Inverters",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Index = table.Column<byte>(nullable: false),
                    IpAddress = table.Column<string>(type: "text", nullable: true),
                    TcpPort = table.Column<int>(nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Inverters", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "IoDevices",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Index = table.Column<byte>(nullable: false),
                    IpAddress = table.Column<string>(type: "text", nullable: true),
                    TcpPort = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IoDevices", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LogEntries",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    BayNumber = table.Column<string>(nullable: true),
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
                    TotalAutomaticTime = table.Column<TimeSpan>(nullable: false),
                    TotalBeltCycles = table.Column<int>(nullable: false),
                    TotalMissionTime = table.Column<TimeSpan>(nullable: false),
                    TotalMovedTrays = table.Column<int>(nullable: false),
                    TotalMovedTraysInBay1 = table.Column<int>(nullable: false),
                    TotalMovedTraysInBay2 = table.Column<int>(nullable: false),
                    TotalMovedTraysInBay3 = table.Column<int>(nullable: false),
                    TotalPowerOnTime = table.Column<TimeSpan>(nullable: false),
                    TotalVerticalAxisCycles = table.Column<int>(nullable: false),
                    TotalVerticalAxisKilometers = table.Column<double>(nullable: false),
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
                    LoadedNetWeight = table.Column<double>(nullable: false),
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
                name: "Errors",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    BayNumber = table.Column<int>(nullable: false),
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
                name: "Shutter",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    InverterId = table.Column<int>(nullable: true),
                    TotalCycles = table.Column<int>(nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Shutter", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Shutter_Inverters_InverterId",
                        column: x => x.InverterId,
                        principalTable: "Inverters",
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
                    Value = table.Column<double>(nullable: false)
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
                name: "BayPosition",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Height = table.Column<double>(nullable: false),
                    BayId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BayPosition", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Bays",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CarouselId = table.Column<int>(nullable: true),
                    ChainOffset = table.Column<double>(nullable: false),
                    CurrentMissionId = table.Column<int>(nullable: true),
                    CurrentMissionOperationId = table.Column<int>(nullable: true),
                    InverterId = table.Column<int>(nullable: true),
                    IoDeviceId = table.Column<int>(nullable: true),
                    IsActive = table.Column<bool>(nullable: false),
                    IsExternal = table.Column<bool>(nullable: false),
                    LoadingUnitId = table.Column<int>(nullable: true),
                    Number = table.Column<int>(nullable: false),
                    Operation = table.Column<int>(nullable: false),
                    ShutterId = table.Column<int>(nullable: true),
                    Side = table.Column<string>(type: "text", nullable: false),
                    MachineId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bays", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Bays_Carousels_CarouselId",
                        column: x => x.CarouselId,
                        principalTable: "Carousels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Bays_Inverters_InverterId",
                        column: x => x.InverterId,
                        principalTable: "Inverters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Bays_IoDevices_IoDeviceId",
                        column: x => x.IoDeviceId,
                        principalTable: "IoDevices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Bays_Shutter_ShutterId",
                        column: x => x.ShutterId,
                        principalTable: "Shutter",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Cells",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PanelId = table.Column<int>(nullable: false),
                    Position = table.Column<double>(nullable: false),
                    Priority = table.Column<int>(nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cells", x => x.Id);
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
                    GrossWeight = table.Column<double>(nullable: false),
                    Height = table.Column<double>(nullable: false),
                    IsIntoMachine = table.Column<bool>(nullable: false),
                    MaxNetWeight = table.Column<double>(nullable: false),
                    MissionsCount = table.Column<int>(nullable: false),
                    Status = table.Column<long>(nullable: false),
                    Tare = table.Column<double>(nullable: false)
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
                name: "Elevators",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    LoadingUnitId = table.Column<int>(nullable: true),
                    StructuralPropertiesId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Elevators", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Elevators_LoadingUnits_LoadingUnitId",
                        column: x => x.LoadingUnitId,
                        principalTable: "LoadingUnits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Elevators_ElevatorStructuralProperties_StructuralPropertiesId",
                        column: x => x.StructuralPropertiesId,
                        principalTable: "ElevatorStructuralProperties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Machines",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ElevatorId = table.Column<int>(nullable: true),
                    Height = table.Column<double>(nullable: false),
                    MaxGrossWeight = table.Column<double>(nullable: false),
                    ModelName = table.Column<string>(nullable: true),
                    SerialNumber = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Machines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Machines_Elevators_ElevatorId",
                        column: x => x.ElevatorId,
                        principalTable: "Elevators",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CellPanels",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Side = table.Column<string>(type: "text", nullable: false),
                    MachineId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CellPanels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CellPanels_Machines_MachineId",
                        column: x => x.MachineId,
                        principalTable: "Machines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MovementProfiles",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Correction = table.Column<double>(nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    TotalDistance = table.Column<double>(nullable: false),
                    ElevatorAxisId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MovementProfiles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MovementParameters",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Acceleration = table.Column<double>(nullable: false),
                    Deceleration = table.Column<double>(nullable: false),
                    Speed = table.Column<double>(nullable: false),
                    Discriminator = table.Column<string>(nullable: false),
                    Number = table.Column<int>(nullable: true),
                    Position = table.Column<double>(nullable: true),
                    MovementProfileId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MovementParameters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MovementParameters_MovementProfiles_MovementProfileId",
                        column: x => x.MovementProfileId,
                        principalTable: "MovementProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ElevatorAxes",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ChainOffset = table.Column<double>(nullable: false),
                    EmptyLoadMovementId = table.Column<int>(nullable: true),
                    InverterId = table.Column<int>(nullable: true),
                    LowerBound = table.Column<double>(nullable: false),
                    MaximumLoadMovementId = table.Column<int>(nullable: true),
                    Offset = table.Column<double>(nullable: false),
                    Orientation = table.Column<int>(nullable: false),
                    Resolution = table.Column<decimal>(nullable: false),
                    TotalCycles = table.Column<int>(nullable: false),
                    UpperBound = table.Column<double>(nullable: false),
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
                        name: "FK_ElevatorAxes_MovementParameters_EmptyLoadMovementId",
                        column: x => x.EmptyLoadMovementId,
                        principalTable: "MovementParameters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ElevatorAxes_Inverters_InverterId",
                        column: x => x.InverterId,
                        principalTable: "Inverters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ElevatorAxes_MovementParameters_MaximumLoadMovementId",
                        column: x => x.MaximumLoadMovementId,
                        principalTable: "MovementParameters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "ErrorDefinitions",
                columns: new[] { "Id", "Code", "Description", "Reason", "Severity" },
                values: new object[] { 100032, 100032, "Cassetto non caricato completamente.", "Il cassetto potrebbe essersi incastrato.", 0 });

            migrationBuilder.InsertData(
                table: "ErrorDefinitions",
                columns: new[] { "Id", "Code", "Description", "Reason", "Severity" },
                values: new object[] { 100033, 100033, "Condizioni per il posizionamento non soddisfatte.", "Controllare che il nottolino sia a zero o che il cassetto sia completamente caricato a bordo elevatore.", 0 });

            migrationBuilder.InsertData(
                table: "ErrorDefinitions",
                columns: new[] { "Id", "Code", "Description", "Reason", "Severity" },
                values: new object[] { 100034, 100034, "Condizioni per la messa in marcia non soddisfatte.", "Controllare che i funghi di mergenza siano disattivati e che tutti i sensori di sicurezza siano disattivi", 0 });

            migrationBuilder.InsertData(
                table: "MachineStatistics",
                columns: new[] { "Id", "TotalAutomaticTime", "TotalBeltCycles", "TotalMissionTime", "TotalMovedTrays", "TotalMovedTraysInBay1", "TotalMovedTraysInBay2", "TotalMovedTraysInBay3", "TotalPowerOnTime", "TotalVerticalAxisCycles", "TotalVerticalAxisKilometers", "WeightCapacityPercentage" },
                values: new object[] { -1, new TimeSpan(0, 0, 0, 0, 0), 0, new TimeSpan(0, 0, 0, 0, 0), 0, 0, 0, 0, new TimeSpan(0, 0, 0, 0, 0), 0, 0.0, 0.0 });

            migrationBuilder.InsertData(
                table: "ServicingInfo",
                columns: new[] { "Id", "InstallationDate", "LastServiceDate", "NextServiceDate", "ServiceStatus" },
                values: new object[] { 1, new DateTime(2016, 12, 9, 16, 46, 29, 820, DateTimeKind.Local).AddTicks(3805), null, null, 86 });

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

            migrationBuilder.InsertData(
                table: "ErrorStatistics",
                columns: new[] { "Code", "TotalErrors" },
                values: new object[] { 100033, 0 });

            migrationBuilder.InsertData(
                table: "ErrorStatistics",
                columns: new[] { "Code", "TotalErrors" },
                values: new object[] { 100034, 0 });

            migrationBuilder.CreateIndex(
                name: "IX_BayPosition_BayId",
                table: "BayPosition",
                column: "BayId");

            migrationBuilder.CreateIndex(
                name: "IX_Bays_CarouselId",
                table: "Bays",
                column: "CarouselId");

            migrationBuilder.CreateIndex(
                name: "IX_Bays_InverterId",
                table: "Bays",
                column: "InverterId");

            migrationBuilder.CreateIndex(
                name: "IX_Bays_IoDeviceId",
                table: "Bays",
                column: "IoDeviceId");

            migrationBuilder.CreateIndex(
                name: "IX_Bays_LoadingUnitId",
                table: "Bays",
                column: "LoadingUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_Bays_MachineId",
                table: "Bays",
                column: "MachineId");

            migrationBuilder.CreateIndex(
                name: "IX_Bays_Number",
                table: "Bays",
                column: "Number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Bays_ShutterId",
                table: "Bays",
                column: "ShutterId");

            migrationBuilder.CreateIndex(
                name: "IX_CellPanels_MachineId",
                table: "CellPanels",
                column: "MachineId");

            migrationBuilder.CreateIndex(
                name: "IX_Cells_PanelId",
                table: "Cells",
                column: "PanelId");

            migrationBuilder.CreateIndex(
                name: "IX_ElevatorAxes_ElevatorId",
                table: "ElevatorAxes",
                column: "ElevatorId");

            migrationBuilder.CreateIndex(
                name: "IX_ElevatorAxes_EmptyLoadMovementId",
                table: "ElevatorAxes",
                column: "EmptyLoadMovementId");

            migrationBuilder.CreateIndex(
                name: "IX_ElevatorAxes_InverterId",
                table: "ElevatorAxes",
                column: "InverterId");

            migrationBuilder.CreateIndex(
                name: "IX_ElevatorAxes_MaximumLoadMovementId",
                table: "ElevatorAxes",
                column: "MaximumLoadMovementId");

            migrationBuilder.CreateIndex(
                name: "IX_Elevators_LoadingUnitId",
                table: "Elevators",
                column: "LoadingUnitId");

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
                column: "Code");

            migrationBuilder.CreateIndex(
                name: "IX_Inverters_Index",
                table: "Inverters",
                column: "Index",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_IoDevices_Index",
                table: "IoDevices",
                column: "Index",
                unique: true);

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
                name: "IX_Machines_ElevatorId",
                table: "Machines",
                column: "ElevatorId");

            migrationBuilder.CreateIndex(
                name: "IX_MovementParameters_MovementProfileId",
                table: "MovementParameters",
                column: "MovementProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_MovementProfiles_ElevatorAxisId",
                table: "MovementProfiles",
                column: "ElevatorAxisId");

            migrationBuilder.CreateIndex(
                name: "IX_MovementProfiles_Name",
                table: "MovementProfiles",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Shutter_InverterId",
                table: "Shutter",
                column: "InverterId");

            migrationBuilder.CreateIndex(
                name: "IX_TorqueCurrentSamples_MeasurementSessionId",
                table: "TorqueCurrentSamples",
                column: "MeasurementSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Name",
                table: "Users",
                column: "Name",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_BayPosition_Bays_BayId",
                table: "BayPosition",
                column: "BayId",
                principalTable: "Bays",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Bays_LoadingUnits_LoadingUnitId",
                table: "Bays",
                column: "LoadingUnitId",
                principalTable: "LoadingUnits",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Bays_Machines_MachineId",
                table: "Bays",
                column: "MachineId",
                principalTable: "Machines",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Cells_CellPanels_PanelId",
                table: "Cells",
                column: "PanelId",
                principalTable: "CellPanels",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MovementProfiles_ElevatorAxes_ElevatorAxisId",
                table: "MovementProfiles",
                column: "ElevatorAxisId",
                principalTable: "ElevatorAxes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ElevatorAxes_Inverters_InverterId",
                table: "ElevatorAxes");

            migrationBuilder.DropForeignKey(
                name: "FK_Elevators_LoadingUnits_LoadingUnitId",
                table: "Elevators");

            migrationBuilder.DropForeignKey(
                name: "FK_ElevatorAxes_Elevators_ElevatorId",
                table: "ElevatorAxes");

            migrationBuilder.DropForeignKey(
                name: "FK_ElevatorAxes_MovementParameters_EmptyLoadMovementId",
                table: "ElevatorAxes");

            migrationBuilder.DropForeignKey(
                name: "FK_ElevatorAxes_MovementParameters_MaximumLoadMovementId",
                table: "ElevatorAxes");

            migrationBuilder.DropTable(
                name: "BayPosition");

            migrationBuilder.DropTable(
                name: "ConfigurationValues");

            migrationBuilder.DropTable(
                name: "Errors");

            migrationBuilder.DropTable(
                name: "ErrorStatistics");

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
                name: "Bays");

            migrationBuilder.DropTable(
                name: "ErrorDefinitions");

            migrationBuilder.DropTable(
                name: "TorqueCurrentMeasurementSessions");

            migrationBuilder.DropTable(
                name: "Carousels");

            migrationBuilder.DropTable(
                name: "IoDevices");

            migrationBuilder.DropTable(
                name: "Shutter");

            migrationBuilder.DropTable(
                name: "Inverters");

            migrationBuilder.DropTable(
                name: "LoadingUnits");

            migrationBuilder.DropTable(
                name: "Cells");

            migrationBuilder.DropTable(
                name: "CellPanels");

            migrationBuilder.DropTable(
                name: "Machines");

            migrationBuilder.DropTable(
                name: "Elevators");

            migrationBuilder.DropTable(
                name: "ElevatorStructuralProperties");

            migrationBuilder.DropTable(
                name: "MovementParameters");

            migrationBuilder.DropTable(
                name: "MovementProfiles");

            migrationBuilder.DropTable(
                name: "ElevatorAxes");
        }
    }
}
