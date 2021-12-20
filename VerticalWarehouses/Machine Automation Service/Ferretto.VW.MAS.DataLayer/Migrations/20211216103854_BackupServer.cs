using Microsoft.EntityFrameworkCore.Migrations;

namespace Ferretto.VW.MAS.DataLayer.Migrations
{
    public partial class BackupServer : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BackupServer",
                table: "Machines",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BackupServerPassword",
                table: "Machines",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BackupServerUsername",
                table: "Machines",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BackupServer",
                table: "Machines");

            migrationBuilder.DropColumn(
                name: "BackupServerPassword",
                table: "Machines");

            migrationBuilder.DropColumn(
                name: "BackupServerUsername",
                table: "Machines");
        }
    }
}
