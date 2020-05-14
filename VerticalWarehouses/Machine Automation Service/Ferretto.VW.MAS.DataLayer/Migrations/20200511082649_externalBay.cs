using Microsoft.EntityFrameworkCore.Migrations;

namespace Ferretto.VW.MAS.DataLayer.Migrations
{
    public partial class externalBay : Migration
    {
        #region Methods

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bays_Externals_ExternalId",
                table: "Bays");

            migrationBuilder.DropTable(
                name: "Externals");

            migrationBuilder.DropTable(
                name: "ExternalBayManualParameters");

            migrationBuilder.DropIndex(
                name: "IX_Bays_ExternalId",
                table: "Bays");

            migrationBuilder.DropColumn(
                name: "ExternalId",
                table: "Bays");
        }

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ExternalId",
                table: "Bays",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ExternalBayManualParameters",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FeedRate = table.Column<double>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExternalBayManualParameters", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Externals",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AssistedMovementsId = table.Column<int>(nullable: true),
                    HomingCreepSpeed = table.Column<double>(nullable: false),
                    HomingFastSpeed = table.Column<double>(nullable: false),
                    LastIdealPosition = table.Column<double>(nullable: false),
                    ManualMovementsId = table.Column<int>(nullable: true),
                    Race = table.Column<double>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Externals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Externals_ExternalBayManualParameters_AssistedMovementsId",
                        column: x => x.AssistedMovementsId,
                        principalTable: "ExternalBayManualParameters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Externals_ExternalBayManualParameters_ManualMovementsId",
                        column: x => x.ManualMovementsId,
                        principalTable: "ExternalBayManualParameters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Bays_ExternalId",
                table: "Bays",
                column: "ExternalId");

            migrationBuilder.CreateIndex(
                name: "IX_Externals_AssistedMovementsId",
                table: "Externals",
                column: "AssistedMovementsId");

            migrationBuilder.CreateIndex(
                name: "IX_Externals_ManualMovementsId",
                table: "Externals",
                column: "ManualMovementsId");
        }

        #endregion
    }
}
