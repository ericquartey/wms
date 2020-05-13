using Microsoft.EntityFrameworkCore.Migrations;

namespace Ferretto.VW.MAS.DataLayer.Migrations
{
    public partial class change_value_type_on_InverterParameter : Migration
    {
        #region Methods

        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable("InverterParameter", newName: "InverterParameter_old");
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
                    Value = table.Column<string>(nullable: true),
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

            migrationBuilder.Sql("INSERT INTO InverterParameter (Code, DataSet, IsReadOnly, Type, Value, InverterId) SELECT Code, DataSet, IsReadOnly, Type, Value, InverterId FROM InverterParameter_old");

            migrationBuilder.DropTable("InverterParameter_old");
        }

        #endregion
    }
}
