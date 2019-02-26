using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Ferretto.VW.MAS_DataLayer.Migrations
{
    public partial class InitialCreate : Migration
    {
        #region Methods

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                "Cells");

            migrationBuilder.DropTable(
                "ConfigurationValues");

            migrationBuilder.DropTable(
                "FreeBlocks");

            migrationBuilder.DropTable(
                "RuntimeValues");

            migrationBuilder.DropTable(
                "StatusLogs");
        }

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                "Cells",
                table => new
                {
                    CellId = table.Column<Int32>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Coord = table.Column<Decimal>(nullable: false),
                    Priority = table.Column<Int32>(nullable: false),
                    Side = table.Column<Int64>(nullable: false),
                    Status = table.Column<Int64>(nullable: false)
                },
                constraints: table => { table.PrimaryKey("PK_Cells", x => x.CellId); });

            migrationBuilder.CreateTable(
                "ConfigurationValues",
                table => new
                {
                    VarName = table.Column<Int64>(nullable: false),
                    VarValue = table.Column<String>(nullable: true),
                    VarType = table.Column<Int64>(nullable: false)
                },
                constraints: table => { table.PrimaryKey("PK_ConfigurationValues", x => x.VarName); });

            migrationBuilder.CreateTable(
                "FreeBlocks",
                table => new
                {
                    BlockSize = table.Column<Int32>(nullable: false),
                    BookedCellsNumber = table.Column<Int32>(nullable: false),
                    Coord = table.Column<Decimal>(nullable: false),
                    FreeBlockId = table.Column<Int32>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Priority = table.Column<Int32>(nullable: false),
                    Side = table.Column<Int64>(nullable: false),
                    StartCell = table.Column<Int32>(nullable: false)
                },
                constraints: table => { table.PrimaryKey("PK_FreeBlocks", x => x.FreeBlockId); });

            migrationBuilder.CreateTable(
                "RuntimeValues",
                table => new
                {
                    VarName = table.Column<Int64>(nullable: false),
                    VarValue = table.Column<String>(nullable: true),
                    VarType = table.Column<Int64>(nullable: false)
                },
                constraints: table => { table.PrimaryKey("PK_RuntimeValues", x => x.VarName); });

            migrationBuilder.CreateTable(
                "StatusLogs",
                table => new
                {
                    LogMessage = table.Column<String>(nullable: true),
                    StatusLogId = table.Column<Int32>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true)
                },
                constraints: table => { table.PrimaryKey("PK_StatusLogs", x => x.StatusLogId); });
        }

        #endregion
    }
}
