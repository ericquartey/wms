using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Ferretto.VW.MAS.DataLayer.Migrations
{
    public partial class initialcreation : Migration
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
                name: "Errors",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    BayNumber = table.Column<int>(nullable: false),
                    Code = table.Column<int>(nullable: false),
                    DetailCode = table.Column<int>(nullable: false),
                    InverterIndex = table.Column<int>(nullable: false),
                    OccurrenceDate = table.Column<DateTime>(nullable: false),
                    ResolutionDate = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Errors", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErrorStatistics",
                columns: table => new
                {
                    Code = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TotalErrors = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ErrorStatistics", x => x.Code);
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
                    TotalWeightBack = table.Column<double>(nullable: false),
                    TotalWeightFront = table.Column<double>(nullable: false),
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
                    Action = table.Column<int>(nullable: false),
                    BayNotifications = table.Column<int>(nullable: false),
                    CloseShutterBayNumber = table.Column<int>(nullable: false),
                    CreationDate = table.Column<DateTime>(nullable: false),
                    DestinationCellId = table.Column<int>(nullable: true),
                    DeviceNotifications = table.Column<int>(nullable: false),
                    Direction = table.Column<int>(nullable: false),
                    EjectLoadUnit = table.Column<bool>(nullable: false),
                    ErrorCode = table.Column<int>(nullable: false),
                    ErrorMovements = table.Column<int>(nullable: false),
                    LoadUnitCellSourceId = table.Column<int>(nullable: true),
                    LoadUnitDestination = table.Column<int>(nullable: false),
                    LoadUnitId = table.Column<int>(nullable: false),
                    LoadUnitSource = table.Column<string>(type: "text", nullable: false),
                    MissionType = table.Column<string>(type: "text", nullable: false),
                    NeedHomingAxis = table.Column<int>(nullable: false),
                    NeedMovingBackward = table.Column<bool>(nullable: false),
                    OpenShutterPosition = table.Column<int>(nullable: false),
                    Priority = table.Column<int>(nullable: false),
                    RestoreConditions = table.Column<bool>(nullable: false),
                    RestoreStep = table.Column<int>(nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    Step = table.Column<int>(nullable: false),
                    StopReason = table.Column<int>(nullable: false),
                    TargetBay = table.Column<string>(type: "text", nullable: false),
                    WmsId = table.Column<int>(nullable: true)
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
                    InstallationDate = table.Column<DateTime>(nullable: true),
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
                    ProfileCorrectDistance = table.Column<double>(nullable: true),
                    ProfileDegrees = table.Column<double>(nullable: true),
                    ProfileTotalDistance = table.Column<double>(nullable: true),
                    InProgress = table.Column<bool>(nullable: true),
                    Step = table.Column<double>(nullable: true),
                    ReferenceCellId = table.Column<int>(nullable: true),
                    RepeatedTestProcedure_InProgress = table.Column<bool>(nullable: true),
                    PerformedCycles = table.Column<int>(nullable: true),
                    RequiredCycles = table.Column<int>(nullable: true),
                    FinalPosition = table.Column<double>(nullable: true),
                    InitialPosition = table.Column<double>(nullable: true),
                    StartPosition = table.Column<double>(nullable: true)
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
                    Bay2FirstLoadingUnit = table.Column<bool>(nullable: false),
                    Bay2HeightCheck = table.Column<bool>(nullable: false),
                    Bay2Laser = table.Column<bool>(nullable: false),
                    Bay3FirstLoadingUnit = table.Column<bool>(nullable: false),
                    Bay3HeightCheck = table.Column<bool>(nullable: false),
                    Bay3Laser = table.Column<bool>(nullable: false),
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
                name: "WmsSettings",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    IsWmsTimeSyncEnabled = table.Column<bool>(nullable: false),
                    LastWmsTimeSync = table.Column<DateTimeOffset>(nullable: false),
                    TimeSyncIntervalMilliseconds = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WmsSettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Carousels",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AssistedMovementsId = table.Column<int>(nullable: true),
                    ElevatorDistance = table.Column<double>(nullable: false),
                    HomingCreepSpeed = table.Column<double>(nullable: false),
                    HomingFastSpeed = table.Column<double>(nullable: false),
                    LastIdealPosition = table.Column<double>(nullable: false),
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
                name: "SetupProceduresSets",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Bay1CarouselCalibrationId = table.Column<int>(nullable: true),
                    Bay1CarouselTestId = table.Column<int>(nullable: true),
                    Bay1ProfileCheckId = table.Column<int>(nullable: true),
                    Bay1ShutterTestId = table.Column<int>(nullable: true),
                    Bay2CarouselCalibrationId = table.Column<int>(nullable: true),
                    Bay2CarouselTestId = table.Column<int>(nullable: true),
                    Bay2ProfileCheckId = table.Column<int>(nullable: true),
                    Bay2ShutterTestId = table.Column<int>(nullable: true),
                    Bay3CarouselCalibrationId = table.Column<int>(nullable: true),
                    Bay3CarouselTestId = table.Column<int>(nullable: true),
                    Bay3ProfileCheckId = table.Column<int>(nullable: true),
                    Bay3ShutterTestId = table.Column<int>(nullable: true),
                    BeltBurnishingTestId = table.Column<int>(nullable: true),
                    CellPanelsCheckId = table.Column<int>(nullable: true),
                    CellsHeightCheckId = table.Column<int>(nullable: true),
                    DepositAndPickUpTestId = table.Column<int>(nullable: true),
                    LoadFirstDrawerTestId = table.Column<int>(nullable: true),
                    ShutterHeightCheckId = table.Column<int>(nullable: true),
                    VerticalOffsetCalibrationId = table.Column<int>(nullable: true),
                    VerticalOriginCalibrationId = table.Column<int>(nullable: true),
                    VerticalResolutionCalibrationId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SetupProceduresSets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SetupProceduresSets_SetupProcedures_Bay1CarouselCalibrationId",
                        column: x => x.Bay1CarouselCalibrationId,
                        principalTable: "SetupProcedures",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SetupProceduresSets_SetupProcedures_Bay1CarouselTestId",
                        column: x => x.Bay1CarouselTestId,
                        principalTable: "SetupProcedures",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SetupProceduresSets_SetupProcedures_Bay1ProfileCheckId",
                        column: x => x.Bay1ProfileCheckId,
                        principalTable: "SetupProcedures",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SetupProceduresSets_SetupProcedures_Bay1ShutterTestId",
                        column: x => x.Bay1ShutterTestId,
                        principalTable: "SetupProcedures",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SetupProceduresSets_SetupProcedures_Bay2CarouselCalibrationId",
                        column: x => x.Bay2CarouselCalibrationId,
                        principalTable: "SetupProcedures",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SetupProceduresSets_SetupProcedures_Bay2CarouselTestId",
                        column: x => x.Bay2CarouselTestId,
                        principalTable: "SetupProcedures",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SetupProceduresSets_SetupProcedures_Bay2ProfileCheckId",
                        column: x => x.Bay2ProfileCheckId,
                        principalTable: "SetupProcedures",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SetupProceduresSets_SetupProcedures_Bay2ShutterTestId",
                        column: x => x.Bay2ShutterTestId,
                        principalTable: "SetupProcedures",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SetupProceduresSets_SetupProcedures_Bay3CarouselCalibrationId",
                        column: x => x.Bay3CarouselCalibrationId,
                        principalTable: "SetupProcedures",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SetupProceduresSets_SetupProcedures_Bay3CarouselTestId",
                        column: x => x.Bay3CarouselTestId,
                        principalTable: "SetupProcedures",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SetupProceduresSets_SetupProcedures_Bay3ProfileCheckId",
                        column: x => x.Bay3ProfileCheckId,
                        principalTable: "SetupProcedures",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SetupProceduresSets_SetupProcedures_Bay3ShutterTestId",
                        column: x => x.Bay3ShutterTestId,
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
                        name: "FK_SetupProceduresSets_SetupProcedures_VerticalOffsetCalibrationId",
                        column: x => x.VerticalOffsetCalibrationId,
                        principalTable: "SetupProcedures",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SetupProceduresSets_SetupProcedures_VerticalOriginCalibrationId",
                        column: x => x.VerticalOriginCalibrationId,
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
                    LoadUnitMaxHeight = table.Column<double>(nullable: false),
                    LoadUnitMaxNetWeight = table.Column<double>(nullable: false),
                    LoadUnitMinHeight = table.Column<double>(nullable: false),
                    LoadUnitTare = table.Column<double>(nullable: false),
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
                    IsChecked = table.Column<bool>(nullable: false),
                    Side = table.Column<int>(nullable: false),
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
                    BlockLevel = table.Column<int>(nullable: false),
                    IsFree = table.Column<bool>(nullable: false),
                    PanelId = table.Column<int>(nullable: false),
                    Position = table.Column<double>(nullable: false),
                    Priority = table.Column<int>(nullable: false)
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
                    Status = table.Column<string>(type: "text", nullable: false),
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
                    BayId = table.Column<int>(nullable: true),
                    Height = table.Column<double>(nullable: false),
                    LoadingUnitId = table.Column<int>(nullable: true),
                    Location = table.Column<string>(type: "text", nullable: false),
                    MaxDoubleHeight = table.Column<double>(nullable: false),
                    MaxSingleHeight = table.Column<double>(nullable: false),
                    ProfileOffset = table.Column<double>(nullable: false)
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
                name: "Lasers",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    BayId = table.Column<int>(nullable: false),
                    IpAddress = table.Column<string>(type: "text", nullable: true),
                    TcpPort = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Lasers", x => x.Id);
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
                    Side = table.Column<int>(nullable: false),
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
                        name: "FK_Bays_Missions_CurrentMissionId",
                        column: x => x.CurrentMissionId,
                        principalTable: "Missions",
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
                    HomingCreepSpeed = table.Column<double>(nullable: false),
                    HomingFastSpeed = table.Column<double>(nullable: false),
                    InverterId = table.Column<int>(nullable: true),
                    LastIdealPosition = table.Column<double>(nullable: false),
                    LowerBound = table.Column<double>(nullable: false),
                    ManualMovementsId = table.Column<int>(nullable: true),
                    Offset = table.Column<double>(nullable: false),
                    Orientation = table.Column<int>(nullable: false),
                    ProfileCalibrateLength = table.Column<double>(nullable: false),
                    ProfileCalibratePosition = table.Column<int>(nullable: false),
                    ProfileCalibrateSpeed = table.Column<double>(nullable: false),
                    Resolution = table.Column<double>(nullable: false),
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
                table: "ErrorStatistics",
                columns: new[] { "Code", "TotalErrors" },
                values: new object[] { 1, 0 });

            migrationBuilder.InsertData(
                table: "ErrorStatistics",
                columns: new[] { "Code", "TotalErrors" },
                values: new object[] { 60, 0 });

            migrationBuilder.InsertData(
                table: "ErrorStatistics",
                columns: new[] { "Code", "TotalErrors" },
                values: new object[] { 59, 0 });

            migrationBuilder.InsertData(
                table: "ErrorStatistics",
                columns: new[] { "Code", "TotalErrors" },
                values: new object[] { 58, 0 });

            migrationBuilder.InsertData(
                table: "ErrorStatistics",
                columns: new[] { "Code", "TotalErrors" },
                values: new object[] { 57, 0 });

            migrationBuilder.InsertData(
                table: "ErrorStatistics",
                columns: new[] { "Code", "TotalErrors" },
                values: new object[] { 56, 0 });

            migrationBuilder.InsertData(
                table: "ErrorStatistics",
                columns: new[] { "Code", "TotalErrors" },
                values: new object[] { 55, 0 });

            migrationBuilder.InsertData(
                table: "ErrorStatistics",
                columns: new[] { "Code", "TotalErrors" },
                values: new object[] { 54, 0 });

            migrationBuilder.InsertData(
                table: "ErrorStatistics",
                columns: new[] { "Code", "TotalErrors" },
                values: new object[] { 61, 0 });

            migrationBuilder.InsertData(
                table: "ErrorStatistics",
                columns: new[] { "Code", "TotalErrors" },
                values: new object[] { 53, 0 });

            migrationBuilder.InsertData(
                table: "ErrorStatistics",
                columns: new[] { "Code", "TotalErrors" },
                values: new object[] { 51, 0 });

            migrationBuilder.InsertData(
                table: "ErrorStatistics",
                columns: new[] { "Code", "TotalErrors" },
                values: new object[] { 50, 0 });

            migrationBuilder.InsertData(
                table: "ErrorStatistics",
                columns: new[] { "Code", "TotalErrors" },
                values: new object[] { 49, 0 });

            migrationBuilder.InsertData(
                table: "ErrorStatistics",
                columns: new[] { "Code", "TotalErrors" },
                values: new object[] { 48, 0 });

            migrationBuilder.InsertData(
                table: "ErrorStatistics",
                columns: new[] { "Code", "TotalErrors" },
                values: new object[] { 47, 0 });

            migrationBuilder.InsertData(
                table: "ErrorStatistics",
                columns: new[] { "Code", "TotalErrors" },
                values: new object[] { 46, 0 });

            migrationBuilder.InsertData(
                table: "ErrorStatistics",
                columns: new[] { "Code", "TotalErrors" },
                values: new object[] { 45, 0 });

            migrationBuilder.InsertData(
                table: "ErrorStatistics",
                columns: new[] { "Code", "TotalErrors" },
                values: new object[] { 52, 0 });

            migrationBuilder.InsertData(
                table: "ErrorStatistics",
                columns: new[] { "Code", "TotalErrors" },
                values: new object[] { 62, 0 });

            migrationBuilder.InsertData(
                table: "ErrorStatistics",
                columns: new[] { "Code", "TotalErrors" },
                values: new object[] { 63, 0 });

            migrationBuilder.InsertData(
                table: "ErrorStatistics",
                columns: new[] { "Code", "TotalErrors" },
                values: new object[] { 64, 0 });

            migrationBuilder.InsertData(
                table: "ErrorStatistics",
                columns: new[] { "Code", "TotalErrors" },
                values: new object[] { -1, 0 });

            migrationBuilder.InsertData(
                table: "ErrorStatistics",
                columns: new[] { "Code", "TotalErrors" },
                values: new object[] { 1030, 0 });

            migrationBuilder.InsertData(
                table: "ErrorStatistics",
                columns: new[] { "Code", "TotalErrors" },
                values: new object[] { 1020, 0 });

            migrationBuilder.InsertData(
                table: "ErrorStatistics",
                columns: new[] { "Code", "TotalErrors" },
                values: new object[] { 1014, 0 });

            migrationBuilder.InsertData(
                table: "ErrorStatistics",
                columns: new[] { "Code", "TotalErrors" },
                values: new object[] { 1013, 0 });

            migrationBuilder.InsertData(
                table: "ErrorStatistics",
                columns: new[] { "Code", "TotalErrors" },
                values: new object[] { 1011, 0 });

            migrationBuilder.InsertData(
                table: "ErrorStatistics",
                columns: new[] { "Code", "TotalErrors" },
                values: new object[] { 1009, 0 });

            migrationBuilder.InsertData(
                table: "ErrorStatistics",
                columns: new[] { "Code", "TotalErrors" },
                values: new object[] { 1008, 0 });

            migrationBuilder.InsertData(
                table: "ErrorStatistics",
                columns: new[] { "Code", "TotalErrors" },
                values: new object[] { 1007, 0 });

            migrationBuilder.InsertData(
                table: "ErrorStatistics",
                columns: new[] { "Code", "TotalErrors" },
                values: new object[] { 1006, 0 });

            migrationBuilder.InsertData(
                table: "ErrorStatistics",
                columns: new[] { "Code", "TotalErrors" },
                values: new object[] { 1005, 0 });

            migrationBuilder.InsertData(
                table: "ErrorStatistics",
                columns: new[] { "Code", "TotalErrors" },
                values: new object[] { 1004, 0 });

            migrationBuilder.InsertData(
                table: "ErrorStatistics",
                columns: new[] { "Code", "TotalErrors" },
                values: new object[] { 1003, 0 });

            migrationBuilder.InsertData(
                table: "ErrorStatistics",
                columns: new[] { "Code", "TotalErrors" },
                values: new object[] { 1002, 0 });

            migrationBuilder.InsertData(
                table: "ErrorStatistics",
                columns: new[] { "Code", "TotalErrors" },
                values: new object[] { 1001, 0 });

            migrationBuilder.InsertData(
                table: "ErrorStatistics",
                columns: new[] { "Code", "TotalErrors" },
                values: new object[] { 1000, 0 });

            migrationBuilder.InsertData(
                table: "ErrorStatistics",
                columns: new[] { "Code", "TotalErrors" },
                values: new object[] { 65, 0 });

            migrationBuilder.InsertData(
                table: "ErrorStatistics",
                columns: new[] { "Code", "TotalErrors" },
                values: new object[] { 44, 0 });

            migrationBuilder.InsertData(
                table: "ErrorStatistics",
                columns: new[] { "Code", "TotalErrors" },
                values: new object[] { 43, 0 });

            migrationBuilder.InsertData(
                table: "ErrorStatistics",
                columns: new[] { "Code", "TotalErrors" },
                values: new object[] { 42, 0 });

            migrationBuilder.InsertData(
                table: "ErrorStatistics",
                columns: new[] { "Code", "TotalErrors" },
                values: new object[] { 20, 0 });

            migrationBuilder.InsertData(
                table: "ErrorStatistics",
                columns: new[] { "Code", "TotalErrors" },
                values: new object[] { 19, 0 });

            migrationBuilder.InsertData(
                table: "ErrorStatistics",
                columns: new[] { "Code", "TotalErrors" },
                values: new object[] { 18, 0 });

            migrationBuilder.InsertData(
                table: "ErrorStatistics",
                columns: new[] { "Code", "TotalErrors" },
                values: new object[] { 17, 0 });

            migrationBuilder.InsertData(
                table: "ErrorStatistics",
                columns: new[] { "Code", "TotalErrors" },
                values: new object[] { 16, 0 });

            migrationBuilder.InsertData(
                table: "ErrorStatistics",
                columns: new[] { "Code", "TotalErrors" },
                values: new object[] { 15, 0 });

            migrationBuilder.InsertData(
                table: "ErrorStatistics",
                columns: new[] { "Code", "TotalErrors" },
                values: new object[] { 14, 0 });

            migrationBuilder.InsertData(
                table: "ErrorStatistics",
                columns: new[] { "Code", "TotalErrors" },
                values: new object[] { 13, 0 });

            migrationBuilder.InsertData(
                table: "ErrorStatistics",
                columns: new[] { "Code", "TotalErrors" },
                values: new object[] { 12, 0 });

            migrationBuilder.InsertData(
                table: "ErrorStatistics",
                columns: new[] { "Code", "TotalErrors" },
                values: new object[] { 11, 0 });

            migrationBuilder.InsertData(
                table: "ErrorStatistics",
                columns: new[] { "Code", "TotalErrors" },
                values: new object[] { 10, 0 });

            migrationBuilder.InsertData(
                table: "ErrorStatistics",
                columns: new[] { "Code", "TotalErrors" },
                values: new object[] { 9, 0 });

            migrationBuilder.InsertData(
                table: "ErrorStatistics",
                columns: new[] { "Code", "TotalErrors" },
                values: new object[] { 8, 0 });

            migrationBuilder.InsertData(
                table: "ErrorStatistics",
                columns: new[] { "Code", "TotalErrors" },
                values: new object[] { 7, 0 });

            migrationBuilder.InsertData(
                table: "ErrorStatistics",
                columns: new[] { "Code", "TotalErrors" },
                values: new object[] { 6, 0 });

            migrationBuilder.InsertData(
                table: "ErrorStatistics",
                columns: new[] { "Code", "TotalErrors" },
                values: new object[] { 5, 0 });

            migrationBuilder.InsertData(
                table: "ErrorStatistics",
                columns: new[] { "Code", "TotalErrors" },
                values: new object[] { 4, 0 });

            migrationBuilder.InsertData(
                table: "ErrorStatistics",
                columns: new[] { "Code", "TotalErrors" },
                values: new object[] { 3, 0 });

            migrationBuilder.InsertData(
                table: "ErrorStatistics",
                columns: new[] { "Code", "TotalErrors" },
                values: new object[] { 41, 0 });

            migrationBuilder.InsertData(
                table: "ErrorStatistics",
                columns: new[] { "Code", "TotalErrors" },
                values: new object[] { 2, 0 });

            migrationBuilder.InsertData(
                table: "ErrorStatistics",
                columns: new[] { "Code", "TotalErrors" },
                values: new object[] { 21, 0 });

            migrationBuilder.InsertData(
                table: "ErrorStatistics",
                columns: new[] { "Code", "TotalErrors" },
                values: new object[] { 23, 0 });

            migrationBuilder.InsertData(
                table: "ErrorStatistics",
                columns: new[] { "Code", "TotalErrors" },
                values: new object[] { 40, 0 });

            migrationBuilder.InsertData(
                table: "ErrorStatistics",
                columns: new[] { "Code", "TotalErrors" },
                values: new object[] { 39, 0 });

            migrationBuilder.InsertData(
                table: "ErrorStatistics",
                columns: new[] { "Code", "TotalErrors" },
                values: new object[] { 38, 0 });

            migrationBuilder.InsertData(
                table: "ErrorStatistics",
                columns: new[] { "Code", "TotalErrors" },
                values: new object[] { 37, 0 });

            migrationBuilder.InsertData(
                table: "ErrorStatistics",
                columns: new[] { "Code", "TotalErrors" },
                values: new object[] { 36, 0 });

            migrationBuilder.InsertData(
                table: "ErrorStatistics",
                columns: new[] { "Code", "TotalErrors" },
                values: new object[] { 35, 0 });

            migrationBuilder.InsertData(
                table: "ErrorStatistics",
                columns: new[] { "Code", "TotalErrors" },
                values: new object[] { 34, 0 });

            migrationBuilder.InsertData(
                table: "ErrorStatistics",
                columns: new[] { "Code", "TotalErrors" },
                values: new object[] { 33, 0 });

            migrationBuilder.InsertData(
                table: "ErrorStatistics",
                columns: new[] { "Code", "TotalErrors" },
                values: new object[] { 32, 0 });

            migrationBuilder.InsertData(
                table: "ErrorStatistics",
                columns: new[] { "Code", "TotalErrors" },
                values: new object[] { 31, 0 });

            migrationBuilder.InsertData(
                table: "ErrorStatistics",
                columns: new[] { "Code", "TotalErrors" },
                values: new object[] { 30, 0 });

            migrationBuilder.InsertData(
                table: "ErrorStatistics",
                columns: new[] { "Code", "TotalErrors" },
                values: new object[] { 29, 0 });

            migrationBuilder.InsertData(
                table: "ErrorStatistics",
                columns: new[] { "Code", "TotalErrors" },
                values: new object[] { 28, 0 });

            migrationBuilder.InsertData(
                table: "ErrorStatistics",
                columns: new[] { "Code", "TotalErrors" },
                values: new object[] { 27, 0 });

            migrationBuilder.InsertData(
                table: "ErrorStatistics",
                columns: new[] { "Code", "TotalErrors" },
                values: new object[] { 26, 0 });

            migrationBuilder.InsertData(
                table: "ErrorStatistics",
                columns: new[] { "Code", "TotalErrors" },
                values: new object[] { 25, 0 });

            migrationBuilder.InsertData(
                table: "ErrorStatistics",
                columns: new[] { "Code", "TotalErrors" },
                values: new object[] { 24, 0 });

            migrationBuilder.InsertData(
                table: "ErrorStatistics",
                columns: new[] { "Code", "TotalErrors" },
                values: new object[] { 22, 0 });

            migrationBuilder.InsertData(
                table: "MachineStatistics",
                columns: new[] { "Id", "TotalAutomaticTime", "TotalBeltCycles", "TotalMissionTime", "TotalMovedTrays", "TotalMovedTraysInBay1", "TotalMovedTraysInBay2", "TotalMovedTraysInBay3", "TotalPowerOnTime", "TotalVerticalAxisCycles", "TotalVerticalAxisKilometers", "TotalWeightBack", "TotalWeightFront", "WeightCapacityPercentage" },
                values: new object[] { -1, new TimeSpan(0, 0, 0, 0, 0), 0, new TimeSpan(0, 0, 0, 0, 0), 0, 0, 0, 0, new TimeSpan(0, 0, 0, 0, 0), 0, 0.0, 0.0, 0.0, 0.0 });

            migrationBuilder.InsertData(
                table: "SetupStatus",
                columns: new[] { "Id", "AllLoadingUnits", "Bay1FirstLoadingUnit", "Bay1HeightCheck", "Bay1Laser", "Bay2FirstLoadingUnit", "Bay2HeightCheck", "Bay2Laser", "Bay3FirstLoadingUnit", "Bay3HeightCheck", "Bay3Laser", "CompletedDate", "HorizontalHoming", "WeightMeasurement" },
                values: new object[] { 1, false, false, false, false, false, false, false, false, false, false, null, false, false });

            migrationBuilder.InsertData(
                table: "WmsSettings",
                columns: new[] { "Id", "IsWmsTimeSyncEnabled", "LastWmsTimeSync", "TimeSyncIntervalMilliseconds" },
                values: new object[] { -1, true, new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 10000 });

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
                name: "IX_Bays_CurrentMissionId",
                table: "Bays",
                column: "CurrentMissionId");

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
                name: "IX_Lasers_BayId",
                table: "Lasers",
                column: "BayId",
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
                name: "IX_SetupProceduresSets_Bay1CarouselCalibrationId",
                table: "SetupProceduresSets",
                column: "Bay1CarouselCalibrationId");

            migrationBuilder.CreateIndex(
                name: "IX_SetupProceduresSets_Bay1CarouselTestId",
                table: "SetupProceduresSets",
                column: "Bay1CarouselTestId");

            migrationBuilder.CreateIndex(
                name: "IX_SetupProceduresSets_Bay1ProfileCheckId",
                table: "SetupProceduresSets",
                column: "Bay1ProfileCheckId");

            migrationBuilder.CreateIndex(
                name: "IX_SetupProceduresSets_Bay1ShutterTestId",
                table: "SetupProceduresSets",
                column: "Bay1ShutterTestId");

            migrationBuilder.CreateIndex(
                name: "IX_SetupProceduresSets_Bay2CarouselCalibrationId",
                table: "SetupProceduresSets",
                column: "Bay2CarouselCalibrationId");

            migrationBuilder.CreateIndex(
                name: "IX_SetupProceduresSets_Bay2CarouselTestId",
                table: "SetupProceduresSets",
                column: "Bay2CarouselTestId");

            migrationBuilder.CreateIndex(
                name: "IX_SetupProceduresSets_Bay2ProfileCheckId",
                table: "SetupProceduresSets",
                column: "Bay2ProfileCheckId");

            migrationBuilder.CreateIndex(
                name: "IX_SetupProceduresSets_Bay2ShutterTestId",
                table: "SetupProceduresSets",
                column: "Bay2ShutterTestId");

            migrationBuilder.CreateIndex(
                name: "IX_SetupProceduresSets_Bay3CarouselCalibrationId",
                table: "SetupProceduresSets",
                column: "Bay3CarouselCalibrationId");

            migrationBuilder.CreateIndex(
                name: "IX_SetupProceduresSets_Bay3CarouselTestId",
                table: "SetupProceduresSets",
                column: "Bay3CarouselTestId");

            migrationBuilder.CreateIndex(
                name: "IX_SetupProceduresSets_Bay3ProfileCheckId",
                table: "SetupProceduresSets",
                column: "Bay3ProfileCheckId");

            migrationBuilder.CreateIndex(
                name: "IX_SetupProceduresSets_Bay3ShutterTestId",
                table: "SetupProceduresSets",
                column: "Bay3ShutterTestId");

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
                name: "IX_SetupProceduresSets_VerticalOffsetCalibrationId",
                table: "SetupProceduresSets",
                column: "VerticalOffsetCalibrationId");

            migrationBuilder.CreateIndex(
                name: "IX_SetupProceduresSets_VerticalOriginCalibrationId",
                table: "SetupProceduresSets",
                column: "VerticalOriginCalibrationId");

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
                name: "FK_Lasers_Bays_BayId",
                table: "Lasers",
                column: "BayId",
                principalTable: "Bays",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

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
                name: "Lasers");

            migrationBuilder.DropTable(
                name: "LogEntries");

            migrationBuilder.DropTable(
                name: "MachineStatistics");

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
                name: "WmsSettings");

            migrationBuilder.DropTable(
                name: "SetupProcedures");

            migrationBuilder.DropTable(
                name: "TorqueCurrentMeasurementSessions");

            migrationBuilder.DropTable(
                name: "Bays");

            migrationBuilder.DropTable(
                name: "Carousels");

            migrationBuilder.DropTable(
                name: "Missions");

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
