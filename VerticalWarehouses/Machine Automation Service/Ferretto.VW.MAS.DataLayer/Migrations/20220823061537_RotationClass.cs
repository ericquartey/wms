using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Ferretto.VW.MAS.DataLayer.Migrations
{
    public partial class RotationClass : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsRotationClass",
                table: "Machines",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsRotationClassFixed",
                table: "LoadingUnits",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "MissionsCountRotation",
                table: "LoadingUnits",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "RotationClass",
                table: "LoadingUnits",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RotationClass",
                table: "Cells",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RotationClass",
                table: "Bays",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsOptimizeRotationClass",
                table: "AutoCompactingSettings",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "RotationClassSchedule",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DaysCount = table.Column<int>(nullable: false),
                    LastSchedule = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RotationClassSchedule", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RotationClassSchedule");

            migrationBuilder.DropColumn(
                name: "IsRotationClass",
                table: "Machines");

            migrationBuilder.DropColumn(
                name: "IsRotationClassFixed",
                table: "LoadingUnits");

            migrationBuilder.DropColumn(
                name: "MissionsCountRotation",
                table: "LoadingUnits");

            migrationBuilder.DropColumn(
                name: "RotationClass",
                table: "LoadingUnits");

            migrationBuilder.DropColumn(
                name: "RotationClass",
                table: "Cells");

            migrationBuilder.DropColumn(
                name: "RotationClass",
                table: "Bays");

            migrationBuilder.DropColumn(
                name: "IsOptimizeRotationClass",
                table: "AutoCompactingSettings");
        }
    }
}
