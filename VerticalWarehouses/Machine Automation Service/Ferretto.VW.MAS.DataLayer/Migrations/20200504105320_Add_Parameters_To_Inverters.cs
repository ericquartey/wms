using Microsoft.EntityFrameworkCore.Migrations;

namespace Ferretto.VW.MAS.DataLayer.Migrations
{
    public partial class Add_Parameters_To_Inverters : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "InverterParameter",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Code = table.Column<short>(nullable: false),
                    DataSet = table.Column<int>(nullable: false),
                    IsReadOnly = table.Column<bool>(nullable: false),
                    Type = table.Column<string>(nullable: true),
                    Value = table.Column<int>(nullable: false),
                    InverterId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InverterParameter", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InverterParameter_Inverters_InverterId",
                        column: x => x.InverterId,
                        principalTable: "Inverters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "ErrorStatistics",
                columns: new[] { "Code", "TotalErrors" },
                values: new object[] { 74, 0 });

            migrationBuilder.CreateIndex(
                name: "IX_InverterParameter_InverterId",
                table: "InverterParameter",
                column: "InverterId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InverterParameter");

            migrationBuilder.DeleteData(
                table: "ErrorStatistics",
                keyColumn: "Code",
                keyValue: 74);
        }
    }
}
