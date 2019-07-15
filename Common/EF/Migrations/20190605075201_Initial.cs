using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Ferretto.Common.EF.Migrations
{
    [System.CodeDom.Compiler.GeneratedCode("EntityFramework", "v2.2")]
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AbcClasses",
                columns: table => new
                {
                    Id = table.Column<string>(type: "char(1)", nullable: false),
                    Description = table.Column<string>(nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AbcClasses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Areas",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Areas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BayTypes",
                columns: table => new
                {
                    Id = table.Column<string>(type: "char(1)", nullable: false),
                    Description = table.Column<string>(nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BayTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CellConfigurations",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Description = table.Column<string>(nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CellConfigurations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CellHeightClasses",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Description = table.Column<string>(nullable: false),
                    MaxHeight = table.Column<double>(nullable: false),
                    MinHeight = table.Column<double>(nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CellHeightClasses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CellPositions",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Description = table.Column<string>(nullable: false),
                    XOffset = table.Column<double>(nullable: true),
                    YOffset = table.Column<double>(nullable: true),
                    ZOffset = table.Column<double>(nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CellPositions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CellSizeClasses",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Description = table.Column<string>(nullable: false),
                    Length = table.Column<double>(nullable: false),
                    Width = table.Column<double>(nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CellSizeClasses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CellStatuses",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Description = table.Column<string>(nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CellStatuses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CellWeightClasses",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Description = table.Column<string>(nullable: false),
                    MaxWeight = table.Column<int>(nullable: false),
                    MinWeight = table.Column<int>(nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CellWeightClasses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CompartmentStatuses",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Description = table.Column<string>(nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompartmentStatuses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CompartmentTypes",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Height = table.Column<double>(nullable: false),
                    Width = table.Column<double>(nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompartmentTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ItemCategories",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Description = table.Column<string>(nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ItemLists",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Code = table.Column<string>(nullable: false),
                    CreationDate = table.Column<DateTime>(nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CustomerOrderCode = table.Column<string>(nullable: true),
                    CustomerOrderDescription = table.Column<string>(nullable: true),
                    Description = table.Column<string>(nullable: true),
                    ExecutionEndDate = table.Column<DateTime>(nullable: true),
                    FirstExecutionDate = table.Column<DateTime>(nullable: true),
                    ItemListType = table.Column<string>(type: "char(1)", nullable: false),
                    Job = table.Column<string>(nullable: true),
                    LastModificationDate = table.Column<DateTime>(nullable: false, defaultValueSql: "GETUTCDATE()"),
                    Priority = table.Column<int>(nullable: true, defaultValue: 1),
                    ShipmentUnitAssociated = table.Column<bool>(nullable: false),
                    ShipmentUnitCode = table.Column<string>(nullable: true),
                    ShipmentUnitDescription = table.Column<string>(nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemLists", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LoadingUnitHeightClasses",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Description = table.Column<string>(nullable: false),
                    MaxHeight = table.Column<double>(nullable: false),
                    MinHeight = table.Column<double>(nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoadingUnitHeightClasses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LoadingUnitSizeClasses",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    BayForksUnthread = table.Column<double>(nullable: true),
                    BayOffset = table.Column<double>(nullable: true),
                    CellForksUnthread = table.Column<double>(nullable: false),
                    Description = table.Column<string>(nullable: false),
                    Length = table.Column<double>(nullable: false),
                    Lift = table.Column<double>(nullable: true),
                    Width = table.Column<double>(nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoadingUnitSizeClasses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LoadingUnitStatuses",
                columns: table => new
                {
                    Id = table.Column<string>(type: "char(1)", nullable: false),
                    Description = table.Column<string>(nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoadingUnitStatuses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LoadingUnitWeightClasses",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Description = table.Column<string>(nullable: false),
                    MaxWeight = table.Column<int>(nullable: false),
                    MinWeight = table.Column<int>(nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoadingUnitWeightClasses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MachineTypes",
                columns: table => new
                {
                    Id = table.Column<string>(type: "char(1)", nullable: false),
                    Description = table.Column<string>(nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MachineTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MaterialStatuses",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Description = table.Column<string>(nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaterialStatuses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MeasureUnits",
                columns: table => new
                {
                    Id = table.Column<string>(type: "char(2)", nullable: false),
                    Description = table.Column<string>(nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MeasureUnits", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PackageTypes",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Description = table.Column<string>(nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PackageTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Aisles",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    AreaId = table.Column<int>(nullable: false),
                    Columns = table.Column<int>(nullable: true),
                    Floors = table.Column<int>(nullable: true),
                    Name = table.Column<string>(nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Aisles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Aisles_Areas_AreaId",
                        column: x => x.AreaId,
                        principalTable: "Areas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LoadingUnitRanges",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    ActualValue = table.Column<int>(nullable: true),
                    AreaId = table.Column<int>(nullable: false),
                    MaxValue = table.Column<int>(nullable: false),
                    MinValue = table.Column<int>(nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoadingUnitRanges", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LoadingUnitRanges_Areas_AreaId",
                        column: x => x.AreaId,
                        principalTable: "Areas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CellTypes",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CellHeightClassId = table.Column<int>(nullable: false),
                    CellSizeClassId = table.Column<int>(nullable: false),
                    CellWeightClassId = table.Column<int>(nullable: false),
                    Description = table.Column<string>(nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CellTypes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CellTypes_CellHeightClasses_CellHeightClassId",
                        column: x => x.CellHeightClassId,
                        principalTable: "CellHeightClasses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CellTypes_CellSizeClasses_CellSizeClassId",
                        column: x => x.CellSizeClassId,
                        principalTable: "CellSizeClasses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CellTypes_CellWeightClasses_CellWeightClassId",
                        column: x => x.CellWeightClassId,
                        principalTable: "CellWeightClasses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LoadingUnitTypes",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Description = table.Column<string>(nullable: false),
                    EmptyWeight = table.Column<long>(nullable: false, defaultValue: 0L),
                    HasCompartments = table.Column<bool>(nullable: false),
                    LoadingUnitHeightClassId = table.Column<int>(nullable: false),
                    LoadingUnitSizeClassId = table.Column<int>(nullable: false),
                    LoadingUnitWeightClassId = table.Column<int>(nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoadingUnitTypes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LoadingUnitTypes_LoadingUnitHeightClasses_LoadingUnitHeightClassId",
                        column: x => x.LoadingUnitHeightClassId,
                        principalTable: "LoadingUnitHeightClasses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LoadingUnitTypes_LoadingUnitSizeClasses_LoadingUnitSizeClassId",
                        column: x => x.LoadingUnitSizeClassId,
                        principalTable: "LoadingUnitSizeClasses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LoadingUnitTypes_LoadingUnitWeightClasses_LoadingUnitWeightClassId",
                        column: x => x.LoadingUnitWeightClassId,
                        principalTable: "LoadingUnitWeightClasses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Items",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    AbcClassId = table.Column<string>(type: "char(1)", nullable: false),
                    AverageWeight = table.Column<int>(nullable: true),
                    Code = table.Column<string>(nullable: false),
                    CreationDate = table.Column<DateTime>(nullable: false, defaultValueSql: "GETUTCDATE()"),
                    Description = table.Column<string>(nullable: true),
                    FifoTimePick = table.Column<int>(nullable: true),
                    FifoTimePut = table.Column<int>(nullable: true),
                    Height = table.Column<double>(nullable: true),
                    Image = table.Column<string>(nullable: true),
                    InventoryDate = table.Column<DateTime>(nullable: true),
                    InventoryTolerance = table.Column<int>(nullable: true),
                    ItemCategoryId = table.Column<int>(nullable: true),
                    LastModificationDate = table.Column<DateTime>(nullable: false, defaultValueSql: "GETUTCDATE()"),
                    LastPickDate = table.Column<DateTime>(nullable: true),
                    LastPutDate = table.Column<DateTime>(nullable: true),
                    Length = table.Column<double>(nullable: true),
                    ManagementType = table.Column<string>(type: "char(1)", nullable: false),
                    MeasureUnitId = table.Column<string>(nullable: true),
                    Note = table.Column<string>(type: "text", nullable: true),
                    PickTolerance = table.Column<int>(nullable: true),
                    PutTolerance = table.Column<int>(nullable: true),
                    ReorderPoint = table.Column<int>(nullable: true),
                    ReorderQuantity = table.Column<int>(nullable: true),
                    Width = table.Column<double>(nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Items", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Items_AbcClasses_AbcClassId",
                        column: x => x.AbcClassId,
                        principalTable: "AbcClasses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Items_ItemCategories_ItemCategoryId",
                        column: x => x.ItemCategoryId,
                        principalTable: "ItemCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Items_MeasureUnits_MeasureUnitId",
                        column: x => x.MeasureUnitId,
                        principalTable: "MeasureUnits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Machines",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    ActualWeight = table.Column<long>(nullable: true),
                    AisleId = table.Column<int>(nullable: false),
                    AutomaticTime = table.Column<long>(nullable: true),
                    BuildDate = table.Column<DateTime>(nullable: true),
                    CradlesCount = table.Column<int>(nullable: true),
                    CustomerAddress = table.Column<string>(nullable: true),
                    CustomerCity = table.Column<string>(nullable: true),
                    CustomerCode = table.Column<string>(nullable: true),
                    CustomerCountry = table.Column<string>(nullable: true),
                    CustomerName = table.Column<string>(nullable: true),
                    ErrorTime = table.Column<long>(nullable: true),
                    Image = table.Column<string>(nullable: true),
                    InputLoadingUnitsCount = table.Column<long>(nullable: true),
                    InstallationDate = table.Column<DateTime>(nullable: true),
                    LastPowerOn = table.Column<DateTime>(nullable: true),
                    LastServiceDate = table.Column<DateTime>(nullable: true),
                    Latitude = table.Column<double>(nullable: true),
                    LoadingUnitsPerCradle = table.Column<int>(nullable: true),
                    Longitude = table.Column<double>(nullable: true),
                    MachineTypeId = table.Column<string>(type: "char(1)", nullable: false),
                    ManualTime = table.Column<long>(nullable: true),
                    MissionTime = table.Column<long>(nullable: true),
                    Model = table.Column<string>(nullable: true),
                    MovedLoadingUnitsCount = table.Column<long>(nullable: true),
                    NextServiceDate = table.Column<DateTime>(nullable: true),
                    Nickname = table.Column<string>(nullable: false),
                    OutputLoadingUnitsCount = table.Column<long>(nullable: true),
                    PowerOnTime = table.Column<long>(nullable: true),
                    RegistrationNumber = table.Column<string>(nullable: true),
                    ServiceUrl = table.Column<string>(nullable: true),
                    TestDate = table.Column<DateTime>(nullable: true),
                    TotalMaxWeight = table.Column<long>(nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Machines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Machines_Aisles_AisleId",
                        column: x => x.AisleId,
                        principalTable: "Aisles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Machines_MachineTypes_MachineTypeId",
                        column: x => x.MachineTypeId,
                        principalTable: "MachineTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CellConfigurationCellTypes",
                columns: table => new
                {
                    CellConfigurationId = table.Column<int>(nullable: false),
                    CellTypeId = table.Column<int>(nullable: false),
                    Priority = table.Column<int>(nullable: false, defaultValue: 1),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CellConfigurationCellTypes", x => new { x.CellConfigurationId, x.CellTypeId });
                    table.ForeignKey(
                        name: "FK_CellConfigurationCellTypes_CellConfigurations_CellConfigurationId",
                        column: x => x.CellConfigurationId,
                        principalTable: "CellConfigurations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CellConfigurationCellTypes_CellTypes_CellTypeId",
                        column: x => x.CellTypeId,
                        principalTable: "CellTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Cells",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    AbcClassId = table.Column<string>(type: "char(1)", nullable: false),
                    AisleId = table.Column<int>(nullable: false),
                    CellNumber = table.Column<int>(nullable: true),
                    CellStatusId = table.Column<int>(nullable: false),
                    CellTypeId = table.Column<int>(nullable: true),
                    Column = table.Column<int>(nullable: false),
                    Floor = table.Column<int>(nullable: false),
                    Priority = table.Column<int>(nullable: false, defaultValue: 1),
                    Side = table.Column<string>(type: "char(1)", nullable: false),
                    XCoordinate = table.Column<double>(nullable: true),
                    YCoordinate = table.Column<double>(nullable: true),
                    ZCoordinate = table.Column<double>(nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cells", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Cells_AbcClasses_AbcClassId",
                        column: x => x.AbcClassId,
                        principalTable: "AbcClasses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Cells_Aisles_AisleId",
                        column: x => x.AisleId,
                        principalTable: "Aisles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Cells_CellStatuses_CellStatusId",
                        column: x => x.CellStatusId,
                        principalTable: "CellStatuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Cells_CellTypes_CellTypeId",
                        column: x => x.CellTypeId,
                        principalTable: "CellTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CellTotals",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    AisleId = table.Column<int>(nullable: false),
                    CellsNumber = table.Column<int>(nullable: false),
                    CellStatusId = table.Column<int>(nullable: false),
                    CellTypeId = table.Column<int>(nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CellTotals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CellTotals_Aisles_AisleId",
                        column: x => x.AisleId,
                        principalTable: "Aisles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CellTotals_CellStatuses_CellStatusId",
                        column: x => x.CellStatusId,
                        principalTable: "CellStatuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CellTotals_CellTypes_CellTypeId",
                        column: x => x.CellTypeId,
                        principalTable: "CellTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CellTypesAisles",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    AisleId = table.Column<int>(nullable: false),
                    CellTypeId = table.Column<int>(nullable: false),
                    CellTypeTotal = table.Column<int>(nullable: false, defaultValue: 0),
                    Ratio = table.Column<decimal>(type: "decimal(3, 2)", nullable: false, defaultValue: 1m),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CellTypesAisles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CellTypesAisles_Aisles_AisleId",
                        column: x => x.AisleId,
                        principalTable: "Aisles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CellTypesAisles_CellTypes_CellTypeId",
                        column: x => x.CellTypeId,
                        principalTable: "CellTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CellConfigurationCellPositionLoadingUnitTypes",
                columns: table => new
                {
                    CellConfigurationId = table.Column<int>(nullable: false),
                    CellPositionId = table.Column<int>(nullable: false),
                    LoadingUnitTypeId = table.Column<int>(nullable: false),
                    Priority = table.Column<int>(nullable: false, defaultValue: 1),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CellConfigurationCellPositionLoadingUnitTypes", x => new { x.CellPositionId, x.CellConfigurationId, x.LoadingUnitTypeId });
                    table.ForeignKey(
                        name: "FK_CellConfigurationCellPositionLoadingUnitTypes_CellConfigurations_CellConfigurationId",
                        column: x => x.CellConfigurationId,
                        principalTable: "CellConfigurations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CellConfigurationCellPositionLoadingUnitTypes_CellPositions_CellPositionId",
                        column: x => x.CellPositionId,
                        principalTable: "CellPositions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CellConfigurationCellPositionLoadingUnitTypes_LoadingUnitTypes_LoadingUnitTypeId",
                        column: x => x.LoadingUnitTypeId,
                        principalTable: "LoadingUnitTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DefaultLoadingUnits",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    DefaultHandlingParametersCorrection = table.Column<int>(nullable: false),
                    Image = table.Column<string>(nullable: true),
                    IsCellPairingFixed = table.Column<bool>(nullable: false),
                    LoadingUnitTypeId = table.Column<int>(nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DefaultLoadingUnits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DefaultLoadingUnits_LoadingUnitTypes_LoadingUnitTypeId",
                        column: x => x.LoadingUnitTypeId,
                        principalTable: "LoadingUnitTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LoadingUnitTypesAisles",
                columns: table => new
                {
                    AisleId = table.Column<int>(nullable: false),
                    LoadingUnitTypeId = table.Column<int>(nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoadingUnitTypesAisles", x => new { x.AisleId, x.LoadingUnitTypeId });
                    table.ForeignKey(
                        name: "FK_LoadingUnitTypesAisles_Aisles_AisleId",
                        column: x => x.AisleId,
                        principalTable: "Aisles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LoadingUnitTypesAisles_LoadingUnitTypes_LoadingUnitTypeId",
                        column: x => x.LoadingUnitTypeId,
                        principalTable: "LoadingUnitTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ItemListRows",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Code = table.Column<string>(nullable: false),
                    CompletionDate = table.Column<DateTime>(nullable: true),
                    CreationDate = table.Column<DateTime>(nullable: false, defaultValueSql: "GETUTCDATE()"),
                    DispatchedQuantity = table.Column<double>(nullable: false),
                    ItemId = table.Column<int>(nullable: false),
                    ItemListId = table.Column<int>(nullable: false),
                    LastExecutionDate = table.Column<DateTime>(nullable: true),
                    LastModificationDate = table.Column<DateTime>(nullable: false, defaultValueSql: "GETUTCDATE()"),
                    Lot = table.Column<string>(nullable: true),
                    MaterialStatusId = table.Column<int>(nullable: true),
                    PackageTypeId = table.Column<int>(nullable: true),
                    Priority = table.Column<int>(nullable: true),
                    RegistrationNumber = table.Column<string>(nullable: true),
                    RequestedQuantity = table.Column<double>(nullable: false),
                    Status = table.Column<string>(type: "char(1)", nullable: false, defaultValueSql: "'N'"),
                    Sub1 = table.Column<string>(nullable: true),
                    Sub2 = table.Column<string>(nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemListRows", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ItemListRows_Items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ItemListRows_ItemLists_ItemListId",
                        column: x => x.ItemListId,
                        principalTable: "ItemLists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ItemListRows_MaterialStatuses_MaterialStatusId",
                        column: x => x.MaterialStatusId,
                        principalTable: "MaterialStatuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ItemListRows_PackageTypes_PackageTypeId",
                        column: x => x.PackageTypeId,
                        principalTable: "PackageTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ItemsAreas",
                columns: table => new
                {
                    AreaId = table.Column<int>(nullable: false),
                    ItemId = table.Column<int>(nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemsAreas", x => new { x.ItemId, x.AreaId });
                    table.ForeignKey(
                        name: "FK_ItemsAreas_Areas_AreaId",
                        column: x => x.AreaId,
                        principalTable: "Areas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ItemsAreas_Items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ItemsCompartmentTypes",
                columns: table => new
                {
                    CompartmentTypeId = table.Column<int>(nullable: false),
                    ItemId = table.Column<int>(nullable: false),
                    MaxCapacity = table.Column<double>(nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemsCompartmentTypes", x => new { x.CompartmentTypeId, x.ItemId });
                    table.ForeignKey(
                        name: "FK_ItemsCompartmentTypes_CompartmentTypes_CompartmentTypeId",
                        column: x => x.CompartmentTypeId,
                        principalTable: "CompartmentTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ItemsCompartmentTypes_Items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Bays",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    AreaId = table.Column<int>(nullable: false),
                    BayTypeId = table.Column<string>(nullable: true),
                    Description = table.Column<string>(nullable: false),
                    IsActive = table.Column<bool>(nullable: false),
                    LoadingUnitsBufferSize = table.Column<int>(nullable: true),
                    MachineId = table.Column<int>(nullable: true),
                    Priority = table.Column<int>(nullable: false, defaultValueSql: "1"),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bays", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Bays_Areas_AreaId",
                        column: x => x.AreaId,
                        principalTable: "Areas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Bays_BayTypes_BayTypeId",
                        column: x => x.BayTypeId,
                        principalTable: "BayTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Bays_Machines_MachineId",
                        column: x => x.MachineId,
                        principalTable: "Machines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LoadingUnits",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    AbcClassId = table.Column<string>(type: "char(1)", nullable: false),
                    CellId = table.Column<int>(nullable: true),
                    CellPositionId = table.Column<int>(nullable: true),
                    Code = table.Column<string>(nullable: false),
                    CreationDate = table.Column<DateTime>(nullable: false, defaultValueSql: "GETUTCDATE()"),
                    HandlingParametersCorrection = table.Column<int>(nullable: true),
                    Height = table.Column<double>(nullable: false),
                    InCycleCount = table.Column<int>(nullable: false, defaultValue: 0),
                    InventoryDate = table.Column<DateTime>(nullable: true),
                    IsCellPairingFixed = table.Column<bool>(nullable: false),
                    LastHandlingDate = table.Column<DateTime>(nullable: true),
                    LastModificationDate = table.Column<DateTime>(nullable: false, defaultValueSql: "GETUTCDATE()"),
                    LastPickDate = table.Column<DateTime>(nullable: true),
                    LastPutDate = table.Column<DateTime>(nullable: true),
                    LoadingUnitStatusId = table.Column<string>(type: "char(1)", nullable: false),
                    LoadingUnitTypeId = table.Column<int>(nullable: false),
                    Note = table.Column<string>(type: "text", nullable: true),
                    OtherCycleCount = table.Column<int>(nullable: false, defaultValue: 0),
                    OutCycleCount = table.Column<int>(nullable: false, defaultValue: 0),
                    ReferenceType = table.Column<string>(type: "char(1)", nullable: false),
                    Weight = table.Column<int>(nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoadingUnits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LoadingUnits_AbcClasses_AbcClassId",
                        column: x => x.AbcClassId,
                        principalTable: "AbcClasses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LoadingUnits_Cells_CellId",
                        column: x => x.CellId,
                        principalTable: "Cells",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LoadingUnits_CellPositions_CellPositionId",
                        column: x => x.CellPositionId,
                        principalTable: "CellPositions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LoadingUnits_LoadingUnitStatuses_LoadingUnitStatusId",
                        column: x => x.LoadingUnitStatusId,
                        principalTable: "LoadingUnitStatuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LoadingUnits_LoadingUnitTypes_LoadingUnitTypeId",
                        column: x => x.LoadingUnitTypeId,
                        principalTable: "LoadingUnitTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DefaultCompartments",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CompartmentTypeId = table.Column<int>(nullable: false),
                    DefaultLoadingUnitId = table.Column<int>(nullable: false),
                    Image = table.Column<string>(nullable: true),
                    Note = table.Column<string>(type: "text", nullable: true),
                    XPosition = table.Column<double>(nullable: false),
                    YPosition = table.Column<double>(nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DefaultCompartments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DefaultCompartments_CompartmentTypes_CompartmentTypeId",
                        column: x => x.CompartmentTypeId,
                        principalTable: "CompartmentTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DefaultCompartments_DefaultLoadingUnits_DefaultLoadingUnitId",
                        column: x => x.DefaultLoadingUnitId,
                        principalTable: "DefaultLoadingUnits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Compartments",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CompartmentStatusId = table.Column<int>(nullable: true),
                    CompartmentTypeId = table.Column<int>(nullable: false),
                    CreationDate = table.Column<DateTime>(nullable: false, defaultValueSql: "GETUTCDATE()"),
                    FifoStartDate = table.Column<DateTime>(nullable: true),
                    HasRotation = table.Column<bool>(nullable: false),
                    InventoryDate = table.Column<DateTime>(nullable: true),
                    IsItemPairingFixed = table.Column<bool>(nullable: false),
                    ItemId = table.Column<int>(nullable: true),
                    LastModificationDate = table.Column<DateTime>(nullable: false, defaultValueSql: "GETUTCDATE()"),
                    LastPickDate = table.Column<DateTime>(nullable: true),
                    LastPutDate = table.Column<DateTime>(nullable: true),
                    LoadingUnitId = table.Column<int>(nullable: false),
                    Lot = table.Column<string>(nullable: true),
                    MaterialStatusId = table.Column<int>(nullable: true),
                    PackageTypeId = table.Column<int>(nullable: true),
                    RegistrationNumber = table.Column<string>(nullable: true),
                    ReservedForPick = table.Column<double>(nullable: false, defaultValue: 0.0),
                    ReservedToPut = table.Column<double>(nullable: false, defaultValue: 0.0),
                    Stock = table.Column<double>(nullable: false, defaultValue: 0.0),
                    Sub1 = table.Column<string>(nullable: true),
                    Sub2 = table.Column<string>(nullable: true),
                    XPosition = table.Column<double>(nullable: false),
                    YPosition = table.Column<double>(nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Compartments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Compartments_CompartmentStatuses_CompartmentStatusId",
                        column: x => x.CompartmentStatusId,
                        principalTable: "CompartmentStatuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Compartments_CompartmentTypes_CompartmentTypeId",
                        column: x => x.CompartmentTypeId,
                        principalTable: "CompartmentTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Compartments_Items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Compartments_LoadingUnits_LoadingUnitId",
                        column: x => x.LoadingUnitId,
                        principalTable: "LoadingUnits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Compartments_MaterialStatuses_MaterialStatusId",
                        column: x => x.MaterialStatusId,
                        principalTable: "MaterialStatuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Compartments_PackageTypes_PackageTypeId",
                        column: x => x.PackageTypeId,
                        principalTable: "PackageTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SchedulerRequests",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    AreaId = table.Column<int>(nullable: true),
                    BayId = table.Column<int>(nullable: true),
                    CreationDate = table.Column<DateTime>(nullable: false, defaultValueSql: "GETUTCDATE()"),
                    IsInstant = table.Column<bool>(nullable: false),
                    ItemId = table.Column<int>(nullable: true),
                    LastModificationDate = table.Column<DateTime>(nullable: false, defaultValueSql: "GETUTCDATE()"),
                    ListId = table.Column<int>(nullable: true),
                    ListRowId = table.Column<int>(nullable: true),
                    LoadingUnitId = table.Column<int>(nullable: true),
                    LoadingUnitTypeId = table.Column<int>(nullable: true),
                    Lot = table.Column<string>(nullable: true),
                    MaterialStatusId = table.Column<int>(nullable: true),
                    OperationType = table.Column<string>(type: "char(1)", nullable: false),
                    PackageTypeId = table.Column<int>(nullable: true),
                    Priority = table.Column<int>(nullable: true),
                    RegistrationNumber = table.Column<string>(nullable: true),
                    RequestedQuantity = table.Column<double>(nullable: true),
                    ReservedQuantity = table.Column<double>(nullable: true),
                    Status = table.Column<string>(type: "char(1)", nullable: false),
                    Sub1 = table.Column<string>(nullable: true),
                    Sub2 = table.Column<string>(nullable: true),
                    Type = table.Column<string>(type: "char(1)", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SchedulerRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SchedulerRequests_Areas_AreaId",
                        column: x => x.AreaId,
                        principalTable: "Areas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SchedulerRequests_Bays_BayId",
                        column: x => x.BayId,
                        principalTable: "Bays",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SchedulerRequests_Items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SchedulerRequests_ItemLists_ListId",
                        column: x => x.ListId,
                        principalTable: "ItemLists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SchedulerRequests_ItemListRows_ListRowId",
                        column: x => x.ListRowId,
                        principalTable: "ItemListRows",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SchedulerRequests_LoadingUnits_LoadingUnitId",
                        column: x => x.LoadingUnitId,
                        principalTable: "LoadingUnits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SchedulerRequests_LoadingUnitTypes_LoadingUnitTypeId",
                        column: x => x.LoadingUnitTypeId,
                        principalTable: "LoadingUnitTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SchedulerRequests_MaterialStatuses_MaterialStatusId",
                        column: x => x.MaterialStatusId,
                        principalTable: "MaterialStatuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SchedulerRequests_PackageTypes_PackageTypeId",
                        column: x => x.PackageTypeId,
                        principalTable: "PackageTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Missions",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    BayId = table.Column<int>(nullable: true),
                    CellId = table.Column<int>(nullable: true),
                    CompartmentId = table.Column<int>(nullable: true),
                    CreationDate = table.Column<DateTime>(nullable: false, defaultValueSql: "GETUTCDATE()"),
                    DispatchedQuantity = table.Column<double>(nullable: false),
                    ItemId = table.Column<int>(nullable: true),
                    ItemListId = table.Column<int>(nullable: true),
                    ItemListRowId = table.Column<int>(nullable: true),
                    LastModificationDate = table.Column<DateTime>(nullable: false, defaultValueSql: "GETUTCDATE()"),
                    LoadingUnitId = table.Column<int>(nullable: true),
                    Lot = table.Column<string>(nullable: true),
                    MaterialStatusId = table.Column<int>(nullable: true),
                    PackageTypeId = table.Column<int>(nullable: true),
                    Priority = table.Column<int>(nullable: false),
                    RegistrationNumber = table.Column<string>(nullable: true),
                    RequestedQuantity = table.Column<double>(nullable: false),
                    Status = table.Column<string>(type: "char(1)", nullable: false, defaultValueSql: "'N'"),
                    Sub1 = table.Column<string>(nullable: true),
                    Sub2 = table.Column<string>(nullable: true),
                    Type = table.Column<string>(type: "char(1)", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Missions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Missions_Bays_BayId",
                        column: x => x.BayId,
                        principalTable: "Bays",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Missions_Cells_CellId",
                        column: x => x.CellId,
                        principalTable: "Cells",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Missions_Compartments_CompartmentId",
                        column: x => x.CompartmentId,
                        principalTable: "Compartments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Missions_Items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Missions_ItemLists_ItemListId",
                        column: x => x.ItemListId,
                        principalTable: "ItemLists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Missions_ItemListRows_ItemListRowId",
                        column: x => x.ItemListRowId,
                        principalTable: "ItemListRows",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Missions_LoadingUnits_LoadingUnitId",
                        column: x => x.LoadingUnitId,
                        principalTable: "LoadingUnits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Missions_MaterialStatuses_MaterialStatusId",
                        column: x => x.MaterialStatusId,
                        principalTable: "MaterialStatuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Missions_PackageTypes_PackageTypeId",
                        column: x => x.PackageTypeId,
                        principalTable: "PackageTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Aisles_AreaId",
                table: "Aisles",
                column: "AreaId");

            migrationBuilder.CreateIndex(
                name: "IX_Bays_AreaId",
                table: "Bays",
                column: "AreaId");

            migrationBuilder.CreateIndex(
                name: "IX_Bays_BayTypeId",
                table: "Bays",
                column: "BayTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Bays_MachineId",
                table: "Bays",
                column: "MachineId");

            migrationBuilder.CreateIndex(
                name: "IX_CellConfigurationCellPositionLoadingUnitTypes_CellConfigurationId",
                table: "CellConfigurationCellPositionLoadingUnitTypes",
                column: "CellConfigurationId");

            migrationBuilder.CreateIndex(
                name: "IX_CellConfigurationCellPositionLoadingUnitTypes_LoadingUnitTypeId",
                table: "CellConfigurationCellPositionLoadingUnitTypes",
                column: "LoadingUnitTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_CellConfigurationCellTypes_CellTypeId",
                table: "CellConfigurationCellTypes",
                column: "CellTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Cells_AbcClassId",
                table: "Cells",
                column: "AbcClassId");

            migrationBuilder.CreateIndex(
                name: "IX_Cells_AisleId",
                table: "Cells",
                column: "AisleId");

            migrationBuilder.CreateIndex(
                name: "IX_Cells_CellStatusId",
                table: "Cells",
                column: "CellStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_Cells_CellTypeId",
                table: "Cells",
                column: "CellTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_CellTotals_AisleId",
                table: "CellTotals",
                column: "AisleId");

            migrationBuilder.CreateIndex(
                name: "IX_CellTotals_CellStatusId",
                table: "CellTotals",
                column: "CellStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_CellTotals_CellTypeId",
                table: "CellTotals",
                column: "CellTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_CellTypes_CellHeightClassId",
                table: "CellTypes",
                column: "CellHeightClassId");

            migrationBuilder.CreateIndex(
                name: "IX_CellTypes_CellSizeClassId",
                table: "CellTypes",
                column: "CellSizeClassId");

            migrationBuilder.CreateIndex(
                name: "IX_CellTypes_CellWeightClassId",
                table: "CellTypes",
                column: "CellWeightClassId");

            migrationBuilder.CreateIndex(
                name: "IX_CellTypesAisles_AisleId",
                table: "CellTypesAisles",
                column: "AisleId");

            migrationBuilder.CreateIndex(
                name: "IX_CellTypesAisles_CellTypeId",
                table: "CellTypesAisles",
                column: "CellTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Compartments_CompartmentStatusId",
                table: "Compartments",
                column: "CompartmentStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_Compartments_CompartmentTypeId",
                table: "Compartments",
                column: "CompartmentTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Compartments_ItemId",
                table: "Compartments",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_Compartments_LoadingUnitId",
                table: "Compartments",
                column: "LoadingUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_Compartments_MaterialStatusId",
                table: "Compartments",
                column: "MaterialStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_Compartments_PackageTypeId",
                table: "Compartments",
                column: "PackageTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_DefaultCompartments_CompartmentTypeId",
                table: "DefaultCompartments",
                column: "CompartmentTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_DefaultCompartments_DefaultLoadingUnitId",
                table: "DefaultCompartments",
                column: "DefaultLoadingUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_DefaultLoadingUnits_LoadingUnitTypeId",
                table: "DefaultLoadingUnits",
                column: "LoadingUnitTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemListRows_Code",
                table: "ItemListRows",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ItemListRows_ItemId",
                table: "ItemListRows",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemListRows_ItemListId",
                table: "ItemListRows",
                column: "ItemListId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemListRows_MaterialStatusId",
                table: "ItemListRows",
                column: "MaterialStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemListRows_PackageTypeId",
                table: "ItemListRows",
                column: "PackageTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemLists_Code",
                table: "ItemLists",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Items_AbcClassId",
                table: "Items",
                column: "AbcClassId");

            migrationBuilder.CreateIndex(
                name: "IX_Items_Code",
                table: "Items",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Items_ItemCategoryId",
                table: "Items",
                column: "ItemCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Items_MeasureUnitId",
                table: "Items",
                column: "MeasureUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemsAreas_AreaId",
                table: "ItemsAreas",
                column: "AreaId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemsCompartmentTypes_ItemId",
                table: "ItemsCompartmentTypes",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_LoadingUnitRanges_AreaId",
                table: "LoadingUnitRanges",
                column: "AreaId");

            migrationBuilder.CreateIndex(
                name: "IX_LoadingUnits_AbcClassId",
                table: "LoadingUnits",
                column: "AbcClassId");

            migrationBuilder.CreateIndex(
                name: "IX_LoadingUnits_CellId",
                table: "LoadingUnits",
                column: "CellId");

            migrationBuilder.CreateIndex(
                name: "IX_LoadingUnits_CellPositionId",
                table: "LoadingUnits",
                column: "CellPositionId");

            migrationBuilder.CreateIndex(
                name: "IX_LoadingUnits_Code",
                table: "LoadingUnits",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LoadingUnits_LoadingUnitStatusId",
                table: "LoadingUnits",
                column: "LoadingUnitStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_LoadingUnits_LoadingUnitTypeId",
                table: "LoadingUnits",
                column: "LoadingUnitTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_LoadingUnitTypes_LoadingUnitHeightClassId",
                table: "LoadingUnitTypes",
                column: "LoadingUnitHeightClassId");

            migrationBuilder.CreateIndex(
                name: "IX_LoadingUnitTypes_LoadingUnitSizeClassId",
                table: "LoadingUnitTypes",
                column: "LoadingUnitSizeClassId");

            migrationBuilder.CreateIndex(
                name: "IX_LoadingUnitTypes_LoadingUnitWeightClassId",
                table: "LoadingUnitTypes",
                column: "LoadingUnitWeightClassId");

            migrationBuilder.CreateIndex(
                name: "IX_LoadingUnitTypesAisles_LoadingUnitTypeId",
                table: "LoadingUnitTypesAisles",
                column: "LoadingUnitTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Machines_AisleId",
                table: "Machines",
                column: "AisleId");

            migrationBuilder.CreateIndex(
                name: "IX_Machines_MachineTypeId",
                table: "Machines",
                column: "MachineTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Machines_Nickname",
                table: "Machines",
                column: "Nickname",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Missions_BayId",
                table: "Missions",
                column: "BayId");

            migrationBuilder.CreateIndex(
                name: "IX_Missions_CellId",
                table: "Missions",
                column: "CellId");

            migrationBuilder.CreateIndex(
                name: "IX_Missions_CompartmentId",
                table: "Missions",
                column: "CompartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Missions_ItemId",
                table: "Missions",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_Missions_ItemListId",
                table: "Missions",
                column: "ItemListId");

            migrationBuilder.CreateIndex(
                name: "IX_Missions_ItemListRowId",
                table: "Missions",
                column: "ItemListRowId");

            migrationBuilder.CreateIndex(
                name: "IX_Missions_LoadingUnitId",
                table: "Missions",
                column: "LoadingUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_Missions_MaterialStatusId",
                table: "Missions",
                column: "MaterialStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_Missions_PackageTypeId",
                table: "Missions",
                column: "PackageTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_SchedulerRequests_AreaId",
                table: "SchedulerRequests",
                column: "AreaId");

            migrationBuilder.CreateIndex(
                name: "IX_SchedulerRequests_BayId",
                table: "SchedulerRequests",
                column: "BayId");

            migrationBuilder.CreateIndex(
                name: "IX_SchedulerRequests_ItemId",
                table: "SchedulerRequests",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_SchedulerRequests_ListId",
                table: "SchedulerRequests",
                column: "ListId");

            migrationBuilder.CreateIndex(
                name: "IX_SchedulerRequests_ListRowId",
                table: "SchedulerRequests",
                column: "ListRowId");

            migrationBuilder.CreateIndex(
                name: "IX_SchedulerRequests_LoadingUnitId",
                table: "SchedulerRequests",
                column: "LoadingUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_SchedulerRequests_LoadingUnitTypeId",
                table: "SchedulerRequests",
                column: "LoadingUnitTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_SchedulerRequests_MaterialStatusId",
                table: "SchedulerRequests",
                column: "MaterialStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_SchedulerRequests_PackageTypeId",
                table: "SchedulerRequests",
                column: "PackageTypeId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CellConfigurationCellPositionLoadingUnitTypes");

            migrationBuilder.DropTable(
                name: "CellConfigurationCellTypes");

            migrationBuilder.DropTable(
                name: "CellTotals");

            migrationBuilder.DropTable(
                name: "CellTypesAisles");

            migrationBuilder.DropTable(
                name: "DefaultCompartments");

            migrationBuilder.DropTable(
                name: "ItemsAreas");

            migrationBuilder.DropTable(
                name: "ItemsCompartmentTypes");

            migrationBuilder.DropTable(
                name: "LoadingUnitRanges");

            migrationBuilder.DropTable(
                name: "LoadingUnitTypesAisles");

            migrationBuilder.DropTable(
                name: "Missions");

            migrationBuilder.DropTable(
                name: "SchedulerRequests");

            migrationBuilder.DropTable(
                name: "CellConfigurations");

            migrationBuilder.DropTable(
                name: "DefaultLoadingUnits");

            migrationBuilder.DropTable(
                name: "Compartments");

            migrationBuilder.DropTable(
                name: "Bays");

            migrationBuilder.DropTable(
                name: "ItemListRows");

            migrationBuilder.DropTable(
                name: "CompartmentStatuses");

            migrationBuilder.DropTable(
                name: "CompartmentTypes");

            migrationBuilder.DropTable(
                name: "LoadingUnits");

            migrationBuilder.DropTable(
                name: "BayTypes");

            migrationBuilder.DropTable(
                name: "Machines");

            migrationBuilder.DropTable(
                name: "Items");

            migrationBuilder.DropTable(
                name: "ItemLists");

            migrationBuilder.DropTable(
                name: "MaterialStatuses");

            migrationBuilder.DropTable(
                name: "PackageTypes");

            migrationBuilder.DropTable(
                name: "Cells");

            migrationBuilder.DropTable(
                name: "CellPositions");

            migrationBuilder.DropTable(
                name: "LoadingUnitStatuses");

            migrationBuilder.DropTable(
                name: "LoadingUnitTypes");

            migrationBuilder.DropTable(
                name: "MachineTypes");

            migrationBuilder.DropTable(
                name: "ItemCategories");

            migrationBuilder.DropTable(
                name: "MeasureUnits");

            migrationBuilder.DropTable(
                name: "AbcClasses");

            migrationBuilder.DropTable(
                name: "Aisles");

            migrationBuilder.DropTable(
                name: "CellStatuses");

            migrationBuilder.DropTable(
                name: "CellTypes");

            migrationBuilder.DropTable(
                name: "LoadingUnitHeightClasses");

            migrationBuilder.DropTable(
                name: "LoadingUnitSizeClasses");

            migrationBuilder.DropTable(
                name: "LoadingUnitWeightClasses");

            migrationBuilder.DropTable(
                name: "Areas");

            migrationBuilder.DropTable(
                name: "CellHeightClasses");

            migrationBuilder.DropTable(
                name: "CellSizeClasses");

            migrationBuilder.DropTable(
                name: "CellWeightClasses");
        }
    }
}
