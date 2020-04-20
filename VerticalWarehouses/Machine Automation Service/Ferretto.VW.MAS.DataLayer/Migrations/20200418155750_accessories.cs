using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Ferretto.VW.MAS.DataLayer.Migrations
{
    public partial class accessories : Migration
    {
        #region Methods

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bays_BayAccessories_AccessoriesId",
                table: "Bays");

            migrationBuilder.DropForeignKey(
                name: "FK_Laser_Bays_BayId",
                table: "Laser");

            migrationBuilder.DropTable(
                name: "BayAccessories");

            migrationBuilder.DropTable(
                name: "Accessories");

            migrationBuilder.DropIndex(
                name: "IX_Bays_AccessoriesId",
                table: "Bays");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Laser",
                table: "Laser");

            migrationBuilder.DropIndex(
                name: "IX_Laser_BayId",
                table: "Laser");

            migrationBuilder.DropColumn(
                name: "AccessoriesId",
                table: "Bays");

            migrationBuilder.RenameTable(
                name: "Laser",
                newName: "Lasers");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Lasers",
                table: "Lasers",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Lasers_BayId",
                table: "Lasers",
                column: "BayId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Lasers_Bays_BayId",
                table: "Lasers",
                column: "BayId",
                principalTable: "Bays",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            if (migrationBuilder is null)
            {
                throw new System.ArgumentNullException(nameof(migrationBuilder));
            }

            this.DropLasersForeignKey(migrationBuilder);

            migrationBuilder.RenameTable(
                name: "Lasers",
                newName: "Laser");

            migrationBuilder.CreateTable(
                name: "Accessories",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    IsConfigured = table.Column<string>(nullable: true),
                    IsEnabled = table.Column<string>(nullable: true),
                    Discriminator = table.Column<string>(nullable: false),
                    PortName = table.Column<string>(nullable: true),
                    Name = table.Column<string>(nullable: true),
                    IpAddress = table.Column<string>(type: "text", nullable: true),
                    TcpPort = table.Column<int>(nullable: true),
                    Size = table.Column<int>(nullable: true),
                    YOffset = table.Column<double>(nullable: true),
                    ZOffsetLowerPosition = table.Column<double>(nullable: true),
                    ZOffsetUpperPosition = table.Column<double>(nullable: true),
                    TokenReader_PortName = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Accessories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BayAccessories",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AlphaNumericBarId = table.Column<int>(nullable: true),
                    BarcodeReaderId = table.Column<int>(nullable: true),
                    CardReaderId = table.Column<int>(nullable: true),
                    LabelPrinterId = table.Column<int>(nullable: true),
                    LaserPointerId = table.Column<int>(nullable: true),
                    TokenReaderId = table.Column<int>(nullable: true),
                    WeightingScaleId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BayAccessories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BayAccessories_Accessories_AlphaNumericBarId",
                        column: x => x.AlphaNumericBarId,
                        principalTable: "Accessories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BayAccessories_Accessories_BarcodeReaderId",
                        column: x => x.BarcodeReaderId,
                        principalTable: "Accessories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BayAccessories_Accessories_CardReaderId",
                        column: x => x.CardReaderId,
                        principalTable: "Accessories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BayAccessories_Accessories_LabelPrinterId",
                        column: x => x.LabelPrinterId,
                        principalTable: "Accessories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BayAccessories_Accessories_LaserPointerId",
                        column: x => x.LaserPointerId,
                        principalTable: "Accessories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BayAccessories_Accessories_TokenReaderId",
                        column: x => x.TokenReaderId,
                        principalTable: "Accessories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BayAccessories_Accessories_WeightingScaleId",
                        column: x => x.WeightingScaleId,
                        principalTable: "Accessories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Bays_AccessoriesId",
                table: "Bays",
                column: "AccessoriesId");

            migrationBuilder.CreateIndex(
                name: "IX_Laser_BayId",
                table: "Laser",
                column: "BayId");

            migrationBuilder.CreateIndex(
                name: "IX_BayAccessories_AlphaNumericBarId",
                table: "BayAccessories",
                column: "AlphaNumericBarId");

            migrationBuilder.CreateIndex(
                name: "IX_BayAccessories_BarcodeReaderId",
                table: "BayAccessories",
                column: "BarcodeReaderId");

            migrationBuilder.CreateIndex(
                name: "IX_BayAccessories_CardReaderId",
                table: "BayAccessories",
                column: "CardReaderId");

            migrationBuilder.CreateIndex(
                name: "IX_BayAccessories_LabelPrinterId",
                table: "BayAccessories",
                column: "LabelPrinterId");

            migrationBuilder.CreateIndex(
                name: "IX_BayAccessories_LaserPointerId",
                table: "BayAccessories",
                column: "LaserPointerId");

            migrationBuilder.CreateIndex(
                name: "IX_BayAccessories_TokenReaderId",
                table: "BayAccessories",
                column: "TokenReaderId");

            migrationBuilder.CreateIndex(
                name: "IX_BayAccessories_WeightingScaleId",
                table: "BayAccessories",
                column: "WeightingScaleId");

            this.AddBayAccessoriesForeignKey(migrationBuilder);
        }

        private void AddBayAccessoriesForeignKey(MigrationBuilder migrationBuilder)
        {
            /*
             * This method replaces these unsupported SQLite operations:

              migrationBuilder.AddForeignKey(
                  name: "FK_Bays_BayAccessories_AccessoriesId",
                  table: "Bays",
                  column: "AccessoriesId",
                  principalTable: "BayAccessories",
                  principalColumn: "Id",
                  onDelete: ReferentialAction.Restrict);
              */

            const string TableName = "Bays";
            const string TempTableName = "temp_Bays";

            migrationBuilder.CreateTable(
                name: "new_Bays",
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
                    MachineId = table.Column<int>(nullable: true),
                    AccessoriesId = table.Column<int>(nullable: true)
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
                    table.ForeignKey(
                        name: "FK_Bays_BayAccessories_AccessoriesId",
                        column: x => x.AccessoriesId,
                        principalTable: "BayAccessories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.Sql(
              $"INSERT INTO new_Bays (Id, CarouselId, ChainOffset, CurrentMissionId, EmptyLoadMovementId, FullLoadMovementId, InverterId, IoDeviceId, IsActive, IsExternal, Number, Operation, Resolution, ShutterId, Side, MachineId) SELECT Id, CarouselId, ChainOffset, CurrentMissionId, EmptyLoadMovementId, FullLoadMovementId, InverterId, IoDeviceId, IsActive, IsExternal, Number, Operation, Resolution, ShutterId, Side, MachineId FROM Bays");

            migrationBuilder.DropTable("Bays");

            migrationBuilder.RenameTable(name: "new_Bays", newName: "Bays");
        }

        private void DropLasersForeignKey(MigrationBuilder migrationBuilder)
        {
            /*
             * This method replaces these unsupported SQLite operations:

                migrationBuilder.DropForeignKey(
                    name: "FK_Lasers_Bays_BayId",
                    table: "Lasers");

                migrationBuilder.DropPrimaryKey(
                   name: "PK_Lasers",
                   table: "Lasers");

                migrationBuilder.DropIndex(
                    name: "IX_Lasers_BayId",
                    table: "Lasers");

                migrationBuilder.AddPrimaryKey(
                    name: "PK_Laser",
                    table: "Laser",
                    column: "Id");

               migrationBuilder.AddForeignKey(
                    name: "FK_Laser_Bays_BayId",
                    table: "Laser",
                    column: "BayId",
                    principalTable: "Bays",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            */

            const string TableName = "Lasers";
            const string TempTableName = "temp_Lasers";

            migrationBuilder.RenameTable(name: TableName, newName: TempTableName);

            migrationBuilder.CreateTable(
               name: TableName,
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
                    table.PrimaryKey("PK_Laser", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Laser_Bays_BayId",
                        column: x => x.BayId,
                        principalTable: "Bays",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.Sql($"INSERT INTO {TableName} SELECT * FROM {TempTableName}");

            migrationBuilder.DropTable(TempTableName);
        }

        #endregion
    }
}
