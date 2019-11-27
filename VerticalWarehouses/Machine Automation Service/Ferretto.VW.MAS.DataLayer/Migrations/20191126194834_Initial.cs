using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Ferretto.VW.MAS.DataLayer.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CarouselManualParameters",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FeedRate = table.Column<double>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CarouselManualParameters", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ElevatorAxisManualParameters",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FeedRate = table.Column<double>(nullable: false),
                    FeedRateAfterZero = table.Column<double>(nullable: false),
                    TargetDistance = table.Column<double>(nullable: false),
                    TargetDistanceAfterZero = table.Column<double>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ElevatorAxisManualParameters", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ElevatorStructuralProperties",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    BeltRigidity = table.Column<int>(nullable: false),
                    BeltSpacing = table.Column<double>(nullable: false),
                    ElevatorWeight = table.Column<double>(nullable: false),
                    HalfShaftLength = table.Column<double>(nullable: false),
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
                    BayNumber = table.Column<int>(nullable: false),
                    Data = table.Column<string>(nullable: true),
                    Description = table.Column<string>(nullable: true),
                    Destination = table.Column<int>(nullable: false),
                    ErrorLevel = table.Column<int>(nullable: false),
                    Source = table.Column<int>(nullable: false),
                    Status = table.Column<int>(nullable: false),
                    TimeStamp = table.Column<DateTime>(nullable: false),
                    Type = table.Column<int>(nullable: false)
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
                name: "Missions",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    BayNumber = table.Column<int>(nullable: false),
                    CreationDate = table.Column<DateTime>(nullable: false),
                    LoadingUnitId = table.Column<int>(nullable: false),
                    Priority = table.Column<int>(nullable: false),
                    Status = table.Column<int>(nullable: false),
                    WmsId = table.Column<int>(nullable: true),
                    WmsPriority = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Missions", x => x.Id);
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
                name: "SetupProcedures",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FeedRate = table.Column<double>(nullable: false),
                    IsCompleted = table.Column<bool>(nullable: false),
                    Discriminator = table.Column<string>(nullable: false),
                    Step = table.Column<double>(nullable: true),
                    ReferenceCellId = table.Column<int>(nullable: true),
                    PerformedCycles = table.Column<int>(nullable: true),
                    RequiredCycles = table.Column<int>(nullable: true),
                    FinalPosition = table.Column<double>(nullable: true),
                    InitialPosition = table.Column<double>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SetupProcedures", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SetupStatus",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AllLoadingUnits = table.Column<bool>(nullable: false),
                    Bay1FirstLoadingUnit = table.Column<bool>(nullable: false),
                    Bay1HeightCheck = table.Column<bool>(nullable: false),
                    Bay1Laser = table.Column<bool>(nullable: false),
                    Bay1Shape = table.Column<bool>(nullable: false),
                    Bay1Shutter = table.Column<bool>(nullable: false),
                    Bay2FirstLoadingUnit = table.Column<bool>(nullable: false),
                    Bay2HeightCheck = table.Column<bool>(nullable: false),
                    Bay2Laser = table.Column<bool>(nullable: false),
                    Bay2Shape = table.Column<bool>(nullable: false),
                    Bay2Shutter = table.Column<bool>(nullable: false),
                    Bay3FirstLoadingUnit = table.Column<bool>(nullable: false),
                    Bay3HeightCheck = table.Column<bool>(nullable: false),
                    Bay3Laser = table.Column<bool>(nullable: false),
                    Bay3Shape = table.Column<bool>(nullable: false),
                    Bay3Shutter = table.Column<bool>(nullable: false),
                    CompletedDate = table.Column<DateTime>(nullable: true),
                    HorizontalHoming = table.Column<bool>(nullable: false),
                    WeightMeasurement = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SetupStatus", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ShutterManualParameters",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FeedRate = table.Column<double>(nullable: false),
                    HighSpeedDurationClose = table.Column<double>(nullable: false),
                    HighSpeedDurationOpen = table.Column<double>(nullable: false),
                    MaxSpeed = table.Column<double>(nullable: false),
                    MinSpeed = table.Column<double>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShutterManualParameters", x => x.Id);
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
                name: "WeightMeasurements",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    MeasureConst0 = table.Column<double>(nullable: false),
                    MeasureConst1 = table.Column<double>(nullable: false),
                    MeasureConst2 = table.Column<double>(nullable: false),
                    MeasureSpeed = table.Column<double>(nullable: false),
                    MeasureTime = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WeightMeasurements", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Carousels",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AssistedMovementsId = table.Column<int>(nullable: true),
                    ElevatorDistance = table.Column<double>(nullable: false),
                    ManualMovementsId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Carousels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Carousels_CarouselManualParameters_AssistedMovementsId",
                        column: x => x.AssistedMovementsId,
                        principalTable: "CarouselManualParameters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Carousels_CarouselManualParameters_ManualMovementsId",
                        column: x => x.ManualMovementsId,
                        principalTable: "CarouselManualParameters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
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
                name: "SetupProceduresSets",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    BayHeightCheckId = table.Column<int>(nullable: true),
                    BeltBurnishingTestId = table.Column<int>(nullable: true),
                    CellPanelsCheckId = table.Column<int>(nullable: true),
                    CellsHeightCheckId = table.Column<int>(nullable: true),
                    DepositAndPickUpTestId = table.Column<int>(nullable: true),
                    LoadFirstDrawerTestId = table.Column<int>(nullable: true),
                    ShutterHeightCheckId = table.Column<int>(nullable: true),
                    ShutterTestId = table.Column<int>(nullable: true),
                    VerticalOffsetCalibrationId = table.Column<int>(nullable: true),
                    VerticalResolutionCalibrationId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SetupProceduresSets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SetupProceduresSets_SetupProcedures_BayHeightCheckId",
                        column: x => x.BayHeightCheckId,
                        principalTable: "SetupProcedures",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SetupProceduresSets_SetupProcedures_BeltBurnishingTestId",
                        column: x => x.BeltBurnishingTestId,
                        principalTable: "SetupProcedures",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SetupProceduresSets_SetupProcedures_CellPanelsCheckId",
                        column: x => x.CellPanelsCheckId,
                        principalTable: "SetupProcedures",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SetupProceduresSets_SetupProcedures_CellsHeightCheckId",
                        column: x => x.CellsHeightCheckId,
                        principalTable: "SetupProcedures",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SetupProceduresSets_SetupProcedures_DepositAndPickUpTestId",
                        column: x => x.DepositAndPickUpTestId,
                        principalTable: "SetupProcedures",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SetupProceduresSets_SetupProcedures_LoadFirstDrawerTestId",
                        column: x => x.LoadFirstDrawerTestId,
                        principalTable: "SetupProcedures",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SetupProceduresSets_SetupProcedures_ShutterHeightCheckId",
                        column: x => x.ShutterHeightCheckId,
                        principalTable: "SetupProcedures",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SetupProceduresSets_SetupProcedures_ShutterTestId",
                        column: x => x.ShutterTestId,
                        principalTable: "SetupProcedures",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SetupProceduresSets_SetupProcedures_VerticalOffsetCalibrationId",
                        column: x => x.VerticalOffsetCalibrationId,
                        principalTable: "SetupProcedures",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SetupProceduresSets_SetupProcedures_VerticalResolutionCalibrationId",
                        column: x => x.VerticalResolutionCalibrationId,
                        principalTable: "SetupProcedures",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Shutters",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AssistedMovementsId = table.Column<int>(nullable: true),
                    InverterId = table.Column<int>(nullable: true),
                    ManualMovementsId = table.Column<int>(nullable: true),
                    Type = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Shutters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Shutters_ShutterManualParameters_AssistedMovementsId",
                        column: x => x.AssistedMovementsId,
                        principalTable: "ShutterManualParameters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Shutters_Inverters_InverterId",
                        column: x => x.InverterId,
                        principalTable: "Inverters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Shutters_ShutterManualParameters_ManualMovementsId",
                        column: x => x.ManualMovementsId,
                        principalTable: "ShutterManualParameters",
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
                name: "Elevators",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    BayPositionId = table.Column<int>(nullable: true),
                    CellId = table.Column<int>(nullable: true),
                    LoadingUnitId = table.Column<int>(nullable: true),
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
                name: "Cells",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    IsDeactivated = table.Column<bool>(nullable: false),
                    IsUnusable = table.Column<bool>(nullable: false),
                    PanelId = table.Column<int>(nullable: false),
                    Position = table.Column<double>(nullable: false),
                    Priority = table.Column<int>(nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false)
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
                name: "LoadingUnits",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CellId = table.Column<int>(nullable: true),
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
                name: "BayPositions",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Height = table.Column<double>(nullable: false),
                    LoadingUnitId = table.Column<int>(nullable: true),
                    Location = table.Column<string>(type: "text", nullable: false),
                    BayId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BayPositions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BayPositions_LoadingUnits_LoadingUnitId",
                        column: x => x.LoadingUnitId,
                        principalTable: "LoadingUnits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
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
                    CurrentWmsMissionOperationId = table.Column<int>(nullable: true),
                    EmptyLoadMovementId = table.Column<int>(nullable: true),
                    FullLoadMovementId = table.Column<int>(nullable: true),
                    InverterId = table.Column<int>(nullable: true),
                    IoDeviceId = table.Column<int>(nullable: true),
                    IsActive = table.Column<bool>(nullable: false),
                    IsExternal = table.Column<bool>(nullable: false),
                    Number = table.Column<int>(nullable: false),
                    Operation = table.Column<int>(nullable: false),
                    Resolution = table.Column<double>(nullable: false),
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
                        name: "FK_Bays_Machines_MachineId",
                        column: x => x.MachineId,
                        principalTable: "Machines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Bays_Shutters_ShutterId",
                        column: x => x.ShutterId,
                        principalTable: "Shutters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MovementProfiles",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
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
                    AdjustByWeight = table.Column<bool>(nullable: true),
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
                    AssistedMovementsId = table.Column<int>(nullable: true),
                    BrakeActivatePercent = table.Column<double>(nullable: false),
                    BrakeReleaseTime = table.Column<double>(nullable: false),
                    ChainOffset = table.Column<double>(nullable: false),
                    EmptyLoadMovementId = table.Column<int>(nullable: true),
                    FullLoadMovementId = table.Column<int>(nullable: true),
                    InverterId = table.Column<int>(nullable: true),
                    LastIdealPosition = table.Column<double>(nullable: false),
                    LowerBound = table.Column<double>(nullable: false),
                    ManualMovementsId = table.Column<int>(nullable: true),
                    Offset = table.Column<double>(nullable: false),
                    Orientation = table.Column<int>(nullable: false),
                    ProfileCalibrateLength = table.Column<double>(nullable: false),
                    ProfileCalibratePosition = table.Column<int>(nullable: false),
                    ProfileCalibrateSpeed = table.Column<double>(nullable: false),
                    Resolution = table.Column<decimal>(nullable: false),
                    TotalCycles = table.Column<int>(nullable: false),
                    UpperBound = table.Column<double>(nullable: false),
                    WeightMeasurementId = table.Column<int>(nullable: true),
                    ElevatorId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ElevatorAxes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ElevatorAxes_ElevatorAxisManualParameters_AssistedMovementsId",
                        column: x => x.AssistedMovementsId,
                        principalTable: "ElevatorAxisManualParameters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
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
                        name: "FK_ElevatorAxes_MovementParameters_FullLoadMovementId",
                        column: x => x.FullLoadMovementId,
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
                        name: "FK_ElevatorAxes_ElevatorAxisManualParameters_ManualMovementsId",
                        column: x => x.ManualMovementsId,
                        principalTable: "ElevatorAxisManualParameters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ElevatorAxes_WeightMeasurements_WeightMeasurementId",
                        column: x => x.WeightMeasurementId,
                        principalTable: "WeightMeasurements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "ErrorDefinitions",
                columns: new[] { "Id", "Code", "Description", "Reason", "Severity" },
                values: new object[] { 1, 1, "Cassetto non caricato completamente.", "Il cassetto potrebbe essersi incastrato.", 0 });

            migrationBuilder.InsertData(
                table: "ErrorDefinitions",
                columns: new[] { "Id", "Code", "Description", "Reason", "Severity" },
                values: new object[] { 200007, 200007, "Errore checksum EEPROM dell'inverter.", "Spegnere e riaccendere la macchina. Se il problema persiste, contattare l'assistenza.", 1 });

            migrationBuilder.InsertData(
                table: "ErrorDefinitions",
                columns: new[] { "Id", "Code", "Description", "Reason", "Severity" },
                values: new object[] { 200008, 200008, "Impossibile scrivere il parametro dell'inverter durante l'esecuzione.", "Spegnere e riaccendere la macchina. Se il problema persiste, contattare l'assistenza.", 1 });

            migrationBuilder.InsertData(
                table: "ErrorDefinitions",
                columns: new[] { "Id", "Code", "Description", "Reason", "Severity" },
                values: new object[] { 200009, 200009, "I dati del dataset dell'inverter non corrispondono.", "Spegnere e riaccendere la macchina. Se il problema persiste, contattare l'assistenza.", 1 });

            migrationBuilder.InsertData(
                table: "ErrorDefinitions",
                columns: new[] { "Id", "Code", "Description", "Reason", "Severity" },
                values: new object[] { 200011, 200011, "Parametro sconosciuto passato all'inverter.", "Spegnere e riaccendere la macchina. Se il problema persiste, contattare l'assistenza.", 1 });

            migrationBuilder.InsertData(
                table: "ErrorDefinitions",
                columns: new[] { "Id", "Code", "Description", "Reason", "Severity" },
                values: new object[] { 200013, 200013, "Errore di sintassi del messaggio inviato all'inverter.", "Spegnere e riaccendere la macchina. Se il problema persiste, contattare l'assistenza.", 1 });

            migrationBuilder.InsertData(
                table: "ErrorDefinitions",
                columns: new[] { "Id", "Code", "Description", "Reason", "Severity" },
                values: new object[] { 200014, 200014, "Incoerenza tra la lunghezza del messaggio all'inverter e il tipo di dato del messaggio.", "Spegnere e riaccendere la macchina. Se il problema persiste, contattare l'assistenza.", 1 });

            migrationBuilder.InsertData(
                table: "ErrorDefinitions",
                columns: new[] { "Id", "Code", "Description", "Reason", "Severity" },
                values: new object[] { 200020, 200020, "Il nodo specificato non è disponibile.", "Spegnere e riaccendere la macchina. Se il problema persiste, contattare l'assistenza.", 1 });

            migrationBuilder.InsertData(
                table: "ErrorDefinitions",
                columns: new[] { "Id", "Code", "Description", "Reason", "Severity" },
                values: new object[] { 200030, 200030, "Errore di sintassi del messaggio inviato all'inverter.", "Spegnere e riaccendere la macchina. Se il problema persiste, contattare l'assistenza.", 1 });

            migrationBuilder.InsertData(
                table: "ErrorDefinitions",
                columns: new[] { "Id", "Code", "Description", "Reason", "Severity" },
                values: new object[] { 300000, 300000, "Errore Machine Manager", "Spegnere e riaccendere la macchina. Se il problema persiste, contattare l'assistenza.", 1 });

            migrationBuilder.InsertData(
                table: "ErrorDefinitions",
                columns: new[] { "Id", "Code", "Description", "Reason", "Severity" },
                values: new object[] { 300001, 300001, "Nessun cassetto presente nella baia indicata", "Assicurarsi che un cassetto sia presente in baia e che i sensori di presenza funzionino correttamente", 1 });

            migrationBuilder.InsertData(
                table: "ErrorDefinitions",
                columns: new[] { "Id", "Code", "Description", "Reason", "Severity" },
                values: new object[] { 200005, 200005, "Errore lettura EEPROM dell'inverter.", "Spegnere e riaccendere la macchina. Se il problema persiste, contattare l'assistenza.", 1 });

            migrationBuilder.InsertData(
                table: "ErrorDefinitions",
                columns: new[] { "Id", "Code", "Description", "Reason", "Severity" },
                values: new object[] { 300002, 300002, "Inconsistenza database posizione sorgente cassetto", "Verificare che la posizione sorgente del cassetto all'interno del database sia correttamente configurata", 1 });

            migrationBuilder.InsertData(
                table: "ErrorDefinitions",
                columns: new[] { "Id", "Code", "Description", "Reason", "Severity" },
                values: new object[] { 300004, 300004, "Culla elevatore occupata", "Verificare che la culla elevatore sia vuota. Verificare il corretto funzionamento dei sensori di presenza cassetto sulla culla.", 1 });

            migrationBuilder.InsertData(
                table: "ErrorDefinitions",
                columns: new[] { "Id", "Code", "Description", "Reason", "Severity" },
                values: new object[] { 300005, 300005, "Cassetto rilevato nella baia di estrazione", "Se il cassetto è stato rimosso controllare i sensori di presenza cassetto in baia, altrimenti rimuovere il cassetto dalla baia.", 1 });

            migrationBuilder.InsertData(
                table: "ErrorDefinitions",
                columns: new[] { "Id", "Code", "Description", "Reason", "Severity" },
                values: new object[] { 300006, 300006, "Baia di destinazione del cassetto occupata", "Verificare che la baia di destinazione del cassetto sia effettivamente vuota. Verificare che i sensori di presenza cassetto in baia funzionino correttamente.", 1 });

            migrationBuilder.InsertData(
                table: "ErrorDefinitions",
                columns: new[] { "Id", "Code", "Description", "Reason", "Severity" },
                values: new object[] { 300007, 300007, "Inconsistenza database cella sorgente cassetto", "Verificare che la cella sorgent del cassetto all'interno del database sia correttamente configurata", 1 });

            migrationBuilder.InsertData(
                table: "ErrorDefinitions",
                columns: new[] { "Id", "Code", "Description", "Reason", "Severity" },
                values: new object[] { 300008, 300008, "Inconsistenza database cassetto", "Il cassetto selezionato non è presente nel database. Verificare il numero cassetto inserito e la corretta configurazione del database.", 1 });

            migrationBuilder.InsertData(
                table: "ErrorDefinitions",
                columns: new[] { "Id", "Code", "Description", "Reason", "Severity" },
                values: new object[] { 300009, 300009, "Il cassetto selezionato non risulta caricato in magazzino", "Il cassetto selezionato risulta presente nel database ma non risulta caricato nel magazzino. Verificare la configurazione del database.", 1 });

            migrationBuilder.InsertData(
                table: "ErrorDefinitions",
                columns: new[] { "Id", "Code", "Description", "Reason", "Severity" },
                values: new object[] { 300010, 300010, "Baia sorgente del cassetto vuota", "Verificare che il cassetto sia effettivamente presente nella baia sorgente. Verificare che i sensori di presenza cassetto in baia funzionino correttamente.", 1 });

            migrationBuilder.InsertData(
                table: "ErrorDefinitions",
                columns: new[] { "Id", "Code", "Description", "Reason", "Severity" },
                values: new object[] { 300011, 300011, "MachineManagerErrorLoadingUnitShutterOpen", "MachineManagerErrorLoadingUnitShutterOpen", 1 });

            migrationBuilder.InsertData(
                table: "ErrorDefinitions",
                columns: new[] { "Id", "Code", "Description", "Reason", "Severity" },
                values: new object[] { 300012, 300012, "MachineManagerErrorLoadingUnitShutterClosed", "MachineManagerErrorLoadingUnitShutterClosed", 1 });

            migrationBuilder.InsertData(
                table: "ErrorDefinitions",
                columns: new[] { "Id", "Code", "Description", "Reason", "Severity" },
                values: new object[] { 300013, 300013, "MachineManagerErrorLoadingUnitPresentInCell", "MachineManagerErrorLoadingUnitPresentInCell", 1 });

            migrationBuilder.InsertData(
                table: "ErrorDefinitions",
                columns: new[] { "Id", "Code", "Description", "Reason", "Severity" },
                values: new object[] { 300003, 300003, "Inconsistenza database cella destinazione cassetto", "Verificare che la cella destinazione del cassetto all'interno del database sia correttamente configurata", 1 });

            migrationBuilder.InsertData(
                table: "ErrorDefinitions",
                columns: new[] { "Id", "Code", "Description", "Reason", "Severity" },
                values: new object[] { 200004, 200004, "Parametro inverter è in sola lettura.", "Spegnere e riaccendere la macchina. Se il problema persiste, contattare l'assistenza.", 1 });

            migrationBuilder.InsertData(
                table: "ErrorDefinitions",
                columns: new[] { "Id", "Code", "Description", "Reason", "Severity" },
                values: new object[] { 200006, 200006, "Errore scrittura EEPROM dell'inverter.", "Spegnere e riaccendere la macchina. Se il problema persiste, contattare l'assistenza.", 1 });

            migrationBuilder.InsertData(
                table: "ErrorDefinitions",
                columns: new[] { "Id", "Code", "Description", "Reason", "Severity" },
                values: new object[] { 200002, 200002, "Dataset inverter non valido.", "Spegnere e riaccendere la macchina. Se il problema persiste, contattare l'assistenza.", 1 });

            migrationBuilder.InsertData(
                table: "ErrorDefinitions",
                columns: new[] { "Id", "Code", "Description", "Reason", "Severity" },
                values: new object[] { 2, 2, "Condizioni per il posizionamento non soddisfatte.", "Controllare che il nottolino sia a zero o che il cassetto sia completamente caricato a bordo elevatore.", 0 });

            migrationBuilder.InsertData(
                table: "ErrorDefinitions",
                columns: new[] { "Id", "Code", "Description", "Reason", "Severity" },
                values: new object[] { 3, 3, "Condizioni per la messa in marcia non soddisfatte.", "Controllare che i funghi di emergenza siano disattivati e che tutti i sensori di sicurezza siano disattivi.", 0 });

            migrationBuilder.InsertData(
                table: "ErrorDefinitions",
                columns: new[] { "Id", "Code", "Description", "Reason", "Severity" },
                values: new object[] { 4, 4, "È scattata la funzione di sicurezza.", "Controllare che i funghi di emergenza siano disattivati e che tutti i sensori di sicurezza siano disattivi.", 0 });

            migrationBuilder.InsertData(
                table: "ErrorDefinitions",
                columns: new[] { "Id", "Code", "Description", "Reason", "Severity" },
                values: new object[] { 5, 5, "È stato rilevato un errore in uno degli inverter.", "Spegnere e riaccendere la macchina. Se il problema persiste, contattare l'assistenza.", 0 });

            migrationBuilder.InsertData(
                table: "ErrorDefinitions",
                columns: new[] { "Id", "Code", "Description", "Reason", "Severity" },
                values: new object[] { 6, 6, "CradleNotCorrectlyLoadedDuringPickup", "Il cassetto sembra non essere completamente a bordo elevatore dopo la fase di carico.", 0 });

            migrationBuilder.InsertData(
                table: "ErrorDefinitions",
                columns: new[] { "Id", "Code", "Description", "Reason", "Severity" },
                values: new object[] { 7, 7, "CradleNotCorrectlyUnloadedDuringDeposit", "Il cassetto non sembra essere completamente fuori dall'elevatore dopo la fase di scarico.", 0 });

            migrationBuilder.InsertData(
                table: "ErrorDefinitions",
                columns: new[] { "Id", "Code", "Description", "Reason", "Severity" },
                values: new object[] { 8, 8, "ZeroSensorErrorAfterPickup", "ZeroSensorErrorAfterPickup", 0 });

            migrationBuilder.InsertData(
                table: "ErrorDefinitions",
                columns: new[] { "Id", "Code", "Description", "Reason", "Severity" },
                values: new object[] { 9, 9, "ZeroSensorErrorAfterDeposit", "ZeroSensorErrorAfterDeposit", 0 });

            migrationBuilder.InsertData(
                table: "ErrorDefinitions",
                columns: new[] { "Id", "Code", "Description", "Reason", "Severity" },
                values: new object[] { 10, 10, "InvalidPresenceSensors", "Sensori di presenza invalidi", 0 });

            migrationBuilder.InsertData(
                table: "ErrorDefinitions",
                columns: new[] { "Id", "Code", "Description", "Reason", "Severity" },
                values: new object[] { 200003, 200003, "Parametro inverter è in sola scrittura.", "Spegnere e riaccendere la macchina. Se il problema persiste, contattare l'assistenza.", 1 });

            migrationBuilder.InsertData(
                table: "ErrorDefinitions",
                columns: new[] { "Id", "Code", "Description", "Reason", "Severity" },
                values: new object[] { 12, 12, "ZeroSensorActiveWithFullElevator", "ZeroSensorActiveWithFullElevator", 0 });

            migrationBuilder.InsertData(
                table: "ErrorDefinitions",
                columns: new[] { "Id", "Code", "Description", "Reason", "Severity" },
                values: new object[] { 11, 11, "MissingZeroSensorWithEmptyElevator", "MissingZeroSensorWithEmptyElevator", 0 });

            migrationBuilder.InsertData(
                table: "ErrorDefinitions",
                columns: new[] { "Id", "Code", "Description", "Reason", "Severity" },
                values: new object[] { 14, 14, "TopLevelBayOccupied", "Livello alto baia occupato", 0 });

            migrationBuilder.InsertData(
                table: "ErrorDefinitions",
                columns: new[] { "Id", "Code", "Description", "Reason", "Severity" },
                values: new object[] { 15, 15, "BottomLevelBayOccupied", "Livello basso baia occupato.", 0 });

            migrationBuilder.InsertData(
                table: "ErrorDefinitions",
                columns: new[] { "Id", "Code", "Description", "Reason", "Severity" },
                values: new object[] { 16, 16, "SensoZeroBayNotActiveAtStart", "SensoZeroBayNotActiveAtStart", 0 });

            migrationBuilder.InsertData(
                table: "ErrorDefinitions",
                columns: new[] { "Id", "Code", "Description", "Reason", "Severity" },
                values: new object[] { 17, 17, "Il peso massimo caricato sul cassetto è eccessivo.", "Scaricare il cassetto in baia e rimuovere il peso in eccesso.", 1 });

            migrationBuilder.InsertData(
                table: "ErrorDefinitions",
                columns: new[] { "Id", "Code", "Description", "Reason", "Severity" },
                values: new object[] { 18, 18, "DestinationBelowLowerBound", "DestinationBelowLowerBound", 1 });

            migrationBuilder.InsertData(
                table: "ErrorDefinitions",
                columns: new[] { "Id", "Code", "Description", "Reason", "Severity" },
                values: new object[] { 19, 19, "DestinationOverUpperBound", "DestinationOverUpperBound", 1 });

            migrationBuilder.InsertData(
                table: "ErrorDefinitions",
                columns: new[] { "Id", "Code", "Description", "Reason", "Severity" },
                values: new object[] { 20, 20, "BayInvertersBusy", "BayInvertersBusy", 1 });

            migrationBuilder.InsertData(
                table: "ErrorDefinitions",
                columns: new[] { "Id", "Code", "Description", "Reason", "Severity" },
                values: new object[] { 21, 21, "IoDeviceError", "IoDeviceError", 1 });

            migrationBuilder.InsertData(
                table: "ErrorDefinitions",
                columns: new[] { "Id", "Code", "Description", "Reason", "Severity" },
                values: new object[] { 200000, 200000, "Errore inverter.", "Spegnere e riaccendere la macchina. Se il problema persiste, contattare l'assistenza.", 1 });

            migrationBuilder.InsertData(
                table: "ErrorDefinitions",
                columns: new[] { "Id", "Code", "Description", "Reason", "Severity" },
                values: new object[] { 200001, 200001, "Paramentro inverter non valido.", "Spegnere e riaccendere la macchina. Se il problema persiste, contattare l'assistenza.", 1 });

            migrationBuilder.InsertData(
                table: "ErrorDefinitions",
                columns: new[] { "Id", "Code", "Description", "Reason", "Severity" },
                values: new object[] { 13, 13, "LoadUnitPresentOnEmptyElevator", "Presenza a bordo elevatore con elevatore logicamente scarico.", 0 });

            migrationBuilder.InsertData(
                table: "MachineStatistics",
                columns: new[] { "Id", "TotalAutomaticTime", "TotalBeltCycles", "TotalMissionTime", "TotalMovedTrays", "TotalMovedTraysInBay1", "TotalMovedTraysInBay2", "TotalMovedTraysInBay3", "TotalPowerOnTime", "TotalVerticalAxisCycles", "TotalVerticalAxisKilometers", "WeightCapacityPercentage" },
                values: new object[] { -1, new TimeSpan(0, 0, 0, 0, 0), 0, new TimeSpan(0, 0, 0, 0, 0), 0, 0, 0, 0, new TimeSpan(0, 0, 0, 0, 0), 0, 0.0, 0.0 });

            migrationBuilder.InsertData(
                table: "ServicingInfo",
                columns: new[] { "Id", "InstallationDate", "LastServiceDate", "NextServiceDate", "ServiceStatus" },
                values: new object[] { 1, new DateTime(2017, 1, 26, 20, 48, 32, 644, DateTimeKind.Local).AddTicks(7425), null, null, 86 });

            migrationBuilder.InsertData(
                table: "SetupStatus",
                columns: new[] { "Id", "AllLoadingUnits", "Bay1FirstLoadingUnit", "Bay1HeightCheck", "Bay1Laser", "Bay1Shape", "Bay1Shutter", "Bay2FirstLoadingUnit", "Bay2HeightCheck", "Bay2Laser", "Bay2Shape", "Bay2Shutter", "Bay3FirstLoadingUnit", "Bay3HeightCheck", "Bay3Laser", "Bay3Shape", "Bay3Shutter", "CompletedDate", "HorizontalHoming", "WeightMeasurement" },
                values: new object[] { 1, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, null, false, false });

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
                values: new object[] { 1, 0 });

            migrationBuilder.InsertData(
                table: "ErrorStatistics",
                columns: new[] { "Code", "TotalErrors" },
                values: new object[] { 200006, 0 });

            migrationBuilder.InsertData(
                table: "ErrorStatistics",
                columns: new[] { "Code", "TotalErrors" },
                values: new object[] { 200007, 0 });

            migrationBuilder.InsertData(
                table: "ErrorStatistics",
                columns: new[] { "Code", "TotalErrors" },
                values: new object[] { 200008, 0 });

            migrationBuilder.InsertData(
                table: "ErrorStatistics",
                columns: new[] { "Code", "TotalErrors" },
                values: new object[] { 200009, 0 });

            migrationBuilder.InsertData(
                table: "ErrorStatistics",
                columns: new[] { "Code", "TotalErrors" },
                values: new object[] { 200011, 0 });

            migrationBuilder.InsertData(
                table: "ErrorStatistics",
                columns: new[] { "Code", "TotalErrors" },
                values: new object[] { 200013, 0 });

            migrationBuilder.InsertData(
                table: "ErrorStatistics",
                columns: new[] { "Code", "TotalErrors" },
                values: new object[] { 200014, 0 });

            migrationBuilder.InsertData(
                table: "ErrorStatistics",
                columns: new[] { "Code", "TotalErrors" },
                values: new object[] { 200020, 0 });

            migrationBuilder.InsertData(
                table: "ErrorStatistics",
                columns: new[] { "Code", "TotalErrors" },
                values: new object[] { 200030, 0 });

            migrationBuilder.InsertData(
                table: "ErrorStatistics",
                columns: new[] { "Code", "TotalErrors" },
                values: new object[] { 300000, 0 });

            migrationBuilder.InsertData(
                table: "ErrorStatistics",
                columns: new[] { "Code", "TotalErrors" },
                values: new object[] { 300001, 0 });

            migrationBuilder.InsertData(
                table: "ErrorStatistics",
                columns: new[] { "Code", "TotalErrors" },
                values: new object[] { 300002, 0 });

            migrationBuilder.InsertData(
                table: "ErrorStatistics",
                columns: new[] { "Code", "TotalErrors" },
                values: new object[] { 300003, 0 });

            migrationBuilder.InsertData(
                table: "ErrorStatistics",
                columns: new[] { "Code", "TotalErrors" },
                values: new object[] { 300004, 0 });

            migrationBuilder.InsertData(
                table: "ErrorStatistics",
                columns: new[] { "Code", "TotalErrors" },
                values: new object[] { 300005, 0 });

            migrationBuilder.InsertData(
                table: "ErrorStatistics",
                columns: new[] { "Code", "TotalErrors" },
                values: new object[] { 300006, 0 });

            migrationBuilder.InsertData(
                table: "ErrorStatistics",
                columns: new[] { "Code", "TotalErrors" },
                values: new object[] { 300007, 0 });

            migrationBuilder.InsertData(
                table: "ErrorStatistics",
                columns: new[] { "Code", "TotalErrors" },
                values: new object[] { 300008, 0 });

            migrationBuilder.InsertData(
                table: "ErrorStatistics",
                columns: new[] { "Code", "TotalErrors" },
                values: new object[] { 300009, 0 });

            migrationBuilder.InsertData(
                table: "ErrorStatistics",
                columns: new[] { "Code", "TotalErrors" },
                values: new object[] { 300010, 0 });

            migrationBuilder.InsertData(
                table: "ErrorStatistics",
                columns: new[] { "Code", "TotalErrors" },
                values: new object[] { 300011, 0 });

            migrationBuilder.InsertData(
                table: "ErrorStatistics",
                columns: new[] { "Code", "TotalErrors" },
                values: new object[] { 200005, 0 });

            migrationBuilder.InsertData(
                table: "ErrorStatistics",
                columns: new[] { "Code", "TotalErrors" },
                values: new object[] { 200004, 0 });

            migrationBuilder.InsertData(
                table: "ErrorStatistics",
                columns: new[] { "Code", "TotalErrors" },
                values: new object[] { 200003, 0 });

            migrationBuilder.InsertData(
                table: "ErrorStatistics",
                columns: new[] { "Code", "TotalErrors" },
                values: new object[] { 200002, 0 });

            migrationBuilder.InsertData(
                table: "ErrorStatistics",
                columns: new[] { "Code", "TotalErrors" },
                values: new object[] { 2, 0 });

            migrationBuilder.InsertData(
                table: "ErrorStatistics",
                columns: new[] { "Code", "TotalErrors" },
                values: new object[] { 3, 0 });

            migrationBuilder.InsertData(
                table: "ErrorStatistics",
                columns: new[] { "Code", "TotalErrors" },
                values: new object[] { 4, 0 });

            migrationBuilder.InsertData(
                table: "ErrorStatistics",
                columns: new[] { "Code", "TotalErrors" },
                values: new object[] { 5, 0 });

            migrationBuilder.InsertData(
                table: "ErrorStatistics",
                columns: new[] { "Code", "TotalErrors" },
                values: new object[] { 6, 0 });

            migrationBuilder.InsertData(
                table: "ErrorStatistics",
                columns: new[] { "Code", "TotalErrors" },
                values: new object[] { 7, 0 });

            migrationBuilder.InsertData(
                table: "ErrorStatistics",
                columns: new[] { "Code", "TotalErrors" },
                values: new object[] { 8, 0 });

            migrationBuilder.InsertData(
                table: "ErrorStatistics",
                columns: new[] { "Code", "TotalErrors" },
                values: new object[] { 9, 0 });

            migrationBuilder.InsertData(
                table: "ErrorStatistics",
                columns: new[] { "Code", "TotalErrors" },
                values: new object[] { 10, 0 });

            migrationBuilder.InsertData(
                table: "ErrorStatistics",
                columns: new[] { "Code", "TotalErrors" },
                values: new object[] { 11, 0 });

            migrationBuilder.InsertData(
                table: "ErrorStatistics",
                columns: new[] { "Code", "TotalErrors" },
                values: new object[] { 300012, 0 });

            migrationBuilder.InsertData(
                table: "ErrorStatistics",
                columns: new[] { "Code", "TotalErrors" },
                values: new object[] { 12, 0 });

            migrationBuilder.InsertData(
                table: "ErrorStatistics",
                columns: new[] { "Code", "TotalErrors" },
                values: new object[] { 14, 0 });

            migrationBuilder.InsertData(
                table: "ErrorStatistics",
                columns: new[] { "Code", "TotalErrors" },
                values: new object[] { 15, 0 });

            migrationBuilder.InsertData(
                table: "ErrorStatistics",
                columns: new[] { "Code", "TotalErrors" },
                values: new object[] { 16, 0 });

            migrationBuilder.InsertData(
                table: "ErrorStatistics",
                columns: new[] { "Code", "TotalErrors" },
                values: new object[] { 17, 0 });

            migrationBuilder.InsertData(
                table: "ErrorStatistics",
                columns: new[] { "Code", "TotalErrors" },
                values: new object[] { 18, 0 });

            migrationBuilder.InsertData(
                table: "ErrorStatistics",
                columns: new[] { "Code", "TotalErrors" },
                values: new object[] { 19, 0 });

            migrationBuilder.InsertData(
                table: "ErrorStatistics",
                columns: new[] { "Code", "TotalErrors" },
                values: new object[] { 20, 0 });

            migrationBuilder.InsertData(
                table: "ErrorStatistics",
                columns: new[] { "Code", "TotalErrors" },
                values: new object[] { 21, 0 });

            migrationBuilder.InsertData(
                table: "ErrorStatistics",
                columns: new[] { "Code", "TotalErrors" },
                values: new object[] { 200000, 0 });

            migrationBuilder.InsertData(
                table: "ErrorStatistics",
                columns: new[] { "Code", "TotalErrors" },
                values: new object[] { 200001, 0 });

            migrationBuilder.InsertData(
                table: "ErrorStatistics",
                columns: new[] { "Code", "TotalErrors" },
                values: new object[] { 13, 0 });

            migrationBuilder.InsertData(
                table: "ErrorStatistics",
                columns: new[] { "Code", "TotalErrors" },
                values: new object[] { 300013, 0 });

            migrationBuilder.CreateIndex(
                name: "IX_BayPositions_BayId",
                table: "BayPositions",
                column: "BayId");

            migrationBuilder.CreateIndex(
                name: "IX_BayPositions_LoadingUnitId",
                table: "BayPositions",
                column: "LoadingUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_Bays_CarouselId",
                table: "Bays",
                column: "CarouselId");

            migrationBuilder.CreateIndex(
                name: "IX_Bays_EmptyLoadMovementId",
                table: "Bays",
                column: "EmptyLoadMovementId");

            migrationBuilder.CreateIndex(
                name: "IX_Bays_FullLoadMovementId",
                table: "Bays",
                column: "FullLoadMovementId");

            migrationBuilder.CreateIndex(
                name: "IX_Bays_InverterId",
                table: "Bays",
                column: "InverterId");

            migrationBuilder.CreateIndex(
                name: "IX_Bays_IoDeviceId",
                table: "Bays",
                column: "IoDeviceId");

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
                name: "IX_Carousels_AssistedMovementsId",
                table: "Carousels",
                column: "AssistedMovementsId");

            migrationBuilder.CreateIndex(
                name: "IX_Carousels_ManualMovementsId",
                table: "Carousels",
                column: "ManualMovementsId");

            migrationBuilder.CreateIndex(
                name: "IX_CellPanels_MachineId",
                table: "CellPanels",
                column: "MachineId");

            migrationBuilder.CreateIndex(
                name: "IX_Cells_PanelId",
                table: "Cells",
                column: "PanelId");

            migrationBuilder.CreateIndex(
                name: "IX_ElevatorAxes_AssistedMovementsId",
                table: "ElevatorAxes",
                column: "AssistedMovementsId");

            migrationBuilder.CreateIndex(
                name: "IX_ElevatorAxes_ElevatorId",
                table: "ElevatorAxes",
                column: "ElevatorId");

            migrationBuilder.CreateIndex(
                name: "IX_ElevatorAxes_EmptyLoadMovementId",
                table: "ElevatorAxes",
                column: "EmptyLoadMovementId");

            migrationBuilder.CreateIndex(
                name: "IX_ElevatorAxes_FullLoadMovementId",
                table: "ElevatorAxes",
                column: "FullLoadMovementId");

            migrationBuilder.CreateIndex(
                name: "IX_ElevatorAxes_InverterId",
                table: "ElevatorAxes",
                column: "InverterId");

            migrationBuilder.CreateIndex(
                name: "IX_ElevatorAxes_ManualMovementsId",
                table: "ElevatorAxes",
                column: "ManualMovementsId");

            migrationBuilder.CreateIndex(
                name: "IX_ElevatorAxes_WeightMeasurementId",
                table: "ElevatorAxes",
                column: "WeightMeasurementId");

            migrationBuilder.CreateIndex(
                name: "IX_Elevators_BayPositionId",
                table: "Elevators",
                column: "BayPositionId");

            migrationBuilder.CreateIndex(
                name: "IX_Elevators_CellId",
                table: "Elevators",
                column: "CellId");

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
                name: "IX_SetupProceduresSets_BayHeightCheckId",
                table: "SetupProceduresSets",
                column: "BayHeightCheckId");

            migrationBuilder.CreateIndex(
                name: "IX_SetupProceduresSets_BeltBurnishingTestId",
                table: "SetupProceduresSets",
                column: "BeltBurnishingTestId");

            migrationBuilder.CreateIndex(
                name: "IX_SetupProceduresSets_CellPanelsCheckId",
                table: "SetupProceduresSets",
                column: "CellPanelsCheckId");

            migrationBuilder.CreateIndex(
                name: "IX_SetupProceduresSets_CellsHeightCheckId",
                table: "SetupProceduresSets",
                column: "CellsHeightCheckId");

            migrationBuilder.CreateIndex(
                name: "IX_SetupProceduresSets_DepositAndPickUpTestId",
                table: "SetupProceduresSets",
                column: "DepositAndPickUpTestId");

            migrationBuilder.CreateIndex(
                name: "IX_SetupProceduresSets_LoadFirstDrawerTestId",
                table: "SetupProceduresSets",
                column: "LoadFirstDrawerTestId");

            migrationBuilder.CreateIndex(
                name: "IX_SetupProceduresSets_ShutterHeightCheckId",
                table: "SetupProceduresSets",
                column: "ShutterHeightCheckId");

            migrationBuilder.CreateIndex(
                name: "IX_SetupProceduresSets_ShutterTestId",
                table: "SetupProceduresSets",
                column: "ShutterTestId");

            migrationBuilder.CreateIndex(
                name: "IX_SetupProceduresSets_VerticalOffsetCalibrationId",
                table: "SetupProceduresSets",
                column: "VerticalOffsetCalibrationId");

            migrationBuilder.CreateIndex(
                name: "IX_SetupProceduresSets_VerticalResolutionCalibrationId",
                table: "SetupProceduresSets",
                column: "VerticalResolutionCalibrationId");

            migrationBuilder.CreateIndex(
                name: "IX_Shutters_AssistedMovementsId",
                table: "Shutters",
                column: "AssistedMovementsId");

            migrationBuilder.CreateIndex(
                name: "IX_Shutters_InverterId",
                table: "Shutters",
                column: "InverterId");

            migrationBuilder.CreateIndex(
                name: "IX_Shutters_ManualMovementsId",
                table: "Shutters",
                column: "ManualMovementsId");

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
                name: "FK_Elevators_LoadingUnits_LoadingUnitId",
                table: "Elevators",
                column: "LoadingUnitId",
                principalTable: "LoadingUnits",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Elevators_BayPositions_BayPositionId",
                table: "Elevators",
                column: "BayPositionId",
                principalTable: "BayPositions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Elevators_Cells_CellId",
                table: "Elevators",
                column: "CellId",
                principalTable: "Cells",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_BayPositions_Bays_BayId",
                table: "BayPositions",
                column: "BayId",
                principalTable: "Bays",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Bays_MovementParameters_EmptyLoadMovementId",
                table: "Bays",
                column: "EmptyLoadMovementId",
                principalTable: "MovementParameters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Bays_MovementParameters_FullLoadMovementId",
                table: "Bays",
                column: "FullLoadMovementId",
                principalTable: "MovementParameters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

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
                name: "FK_BayPositions_Bays_BayId",
                table: "BayPositions");

            migrationBuilder.DropForeignKey(
                name: "FK_BayPositions_LoadingUnits_LoadingUnitId",
                table: "BayPositions");

            migrationBuilder.DropForeignKey(
                name: "FK_Elevators_LoadingUnits_LoadingUnitId",
                table: "Elevators");

            migrationBuilder.DropForeignKey(
                name: "FK_ElevatorAxes_MovementParameters_EmptyLoadMovementId",
                table: "ElevatorAxes");

            migrationBuilder.DropForeignKey(
                name: "FK_ElevatorAxes_MovementParameters_FullLoadMovementId",
                table: "ElevatorAxes");

            migrationBuilder.DropForeignKey(
                name: "FK_CellPanels_Machines_MachineId",
                table: "CellPanels");

            migrationBuilder.DropTable(
                name: "Errors");

            migrationBuilder.DropTable(
                name: "ErrorStatistics");

            migrationBuilder.DropTable(
                name: "LogEntries");

            migrationBuilder.DropTable(
                name: "MachineStatistics");

            migrationBuilder.DropTable(
                name: "Missions");

            migrationBuilder.DropTable(
                name: "ServicingInfo");

            migrationBuilder.DropTable(
                name: "SetupProceduresSets");

            migrationBuilder.DropTable(
                name: "SetupStatus");

            migrationBuilder.DropTable(
                name: "TorqueCurrentSamples");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "ErrorDefinitions");

            migrationBuilder.DropTable(
                name: "SetupProcedures");

            migrationBuilder.DropTable(
                name: "TorqueCurrentMeasurementSessions");

            migrationBuilder.DropTable(
                name: "Bays");

            migrationBuilder.DropTable(
                name: "Carousels");

            migrationBuilder.DropTable(
                name: "IoDevices");

            migrationBuilder.DropTable(
                name: "Shutters");

            migrationBuilder.DropTable(
                name: "CarouselManualParameters");

            migrationBuilder.DropTable(
                name: "ShutterManualParameters");

            migrationBuilder.DropTable(
                name: "LoadingUnits");

            migrationBuilder.DropTable(
                name: "MovementParameters");

            migrationBuilder.DropTable(
                name: "MovementProfiles");

            migrationBuilder.DropTable(
                name: "ElevatorAxes");

            migrationBuilder.DropTable(
                name: "ElevatorAxisManualParameters");

            migrationBuilder.DropTable(
                name: "Inverters");

            migrationBuilder.DropTable(
                name: "WeightMeasurements");

            migrationBuilder.DropTable(
                name: "Machines");

            migrationBuilder.DropTable(
                name: "Elevators");

            migrationBuilder.DropTable(
                name: "BayPositions");

            migrationBuilder.DropTable(
                name: "Cells");

            migrationBuilder.DropTable(
                name: "ElevatorStructuralProperties");

            migrationBuilder.DropTable(
                name: "CellPanels");
        }
    }
}
