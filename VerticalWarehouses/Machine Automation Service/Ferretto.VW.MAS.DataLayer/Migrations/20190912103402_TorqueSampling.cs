using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Ferretto.VW.MAS.DataLayer.Migrations
{
    public partial class TorqueSampling : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                name: "TorqueCurrentSample",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    MeasurementSessionId = table.Column<int>(nullable: false),
                    TimeStamp = table.Column<DateTime>(nullable: false),
                    Value = table.Column<decimal>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TorqueCurrentSample", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TorqueCurrentSample_TorqueCurrentMeasurementSessions_MeasurementSessionId",
                        column: x => x.MeasurementSessionId,
                        principalTable: "TorqueCurrentMeasurementSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "ServicingInfo",
                keyColumn: "Id",
                keyValue: 1,
                column: "InstallationDate",
                value: new DateTime(2016, 11, 12, 12, 34, 1, 367, DateTimeKind.Local).AddTicks(1787));

            migrationBuilder.CreateIndex(
                name: "IX_LoadingUnits_Code",
                table: "LoadingUnits",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TorqueCurrentSample_MeasurementSessionId",
                table: "TorqueCurrentSample",
                column: "MeasurementSessionId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TorqueCurrentSample");

            migrationBuilder.DropTable(
                name: "TorqueCurrentMeasurementSessions");

            migrationBuilder.DropIndex(
                name: "IX_LoadingUnits_Code",
                table: "LoadingUnits");

            migrationBuilder.UpdateData(
                table: "ServicingInfo",
                keyColumn: "Id",
                keyValue: 1,
                column: "InstallationDate",
                value: new DateTime(2016, 11, 5, 15, 2, 36, 548, DateTimeKind.Local).AddTicks(5220));
        }
    }
}
