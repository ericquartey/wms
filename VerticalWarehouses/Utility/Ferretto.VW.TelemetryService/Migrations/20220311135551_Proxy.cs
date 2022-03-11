using Microsoft.EntityFrameworkCore.Migrations;

namespace Ferretto.VW.TelemetryService.Migrations
{
    public partial class Proxy : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Proxys",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PasswordHash = table.Column<string>(nullable: true),
                    PasswordSalt = table.Column<string>(nullable: true),
                    Url = table.Column<string>(nullable: true),
                    User = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Proxys", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Proxys");
        }
    }
}
