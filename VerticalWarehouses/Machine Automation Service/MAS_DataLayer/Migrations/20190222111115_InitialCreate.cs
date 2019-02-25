using Microsoft.EntityFrameworkCore.Migrations;

namespace Ferretto.VW.MAS_DataLayer.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Cells",
                columns: table => new
                {
                    CellId = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Coord = table.Column<decimal>(nullable: false),
                    Priority = table.Column<int>(nullable: false),
                    Side = table.Column<long>(nullable: false),
                    Status = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cells", x => x.CellId);
                });

            migrationBuilder.CreateTable(
                name: "ConfigurationValues",
                columns: table => new
                {
                    VarName = table.Column<long>(nullable: false),
                    VarValue = table.Column<string>(nullable: true),
                    VarType = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConfigurationValues", x => x.VarName);
                });

            migrationBuilder.CreateTable(
                name: "FreeBlocks",
                columns: table => new
                {
                    BlockSize = table.Column<int>(nullable: false),
                    BookedCellsNumber = table.Column<int>(nullable: false),
                    Coord = table.Column<decimal>(nullable: false),
                    FreeBlockId = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Priority = table.Column<int>(nullable: false),
                    Side = table.Column<long>(nullable: false),
                    StartCell = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FreeBlocks", x => x.FreeBlockId);
                });

            migrationBuilder.CreateTable(
                name: "RuntimeValues",
                columns: table => new
                {
                    VarName = table.Column<long>(nullable: false),
                    VarValue = table.Column<string>(nullable: true),
                    VarType = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RuntimeValues", x => x.VarName);
                });

            migrationBuilder.CreateTable(
                name: "StatusLogs",
                columns: table => new
                {
                    LogMessage = table.Column<string>(nullable: true),
                    StatusLogId = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StatusLogs", x => x.StatusLogId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Cells");

            migrationBuilder.DropTable(
                name: "ConfigurationValues");

            migrationBuilder.DropTable(
                name: "FreeBlocks");

            migrationBuilder.DropTable(
                name: "RuntimeValues");

            migrationBuilder.DropTable(
                name: "StatusLogs");
        }
    }
}
