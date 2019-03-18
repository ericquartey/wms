using Microsoft.EntityFrameworkCore.Migrations;

namespace Ferretto.Common.EF.Migrations
{
    [System.CodeDom.Compiler.GeneratedCode("EntityFramework", "v2.1")]
    public partial class Add_Fields_Area_Machine_To_Table_Bays : Migration
    {
        #region Methods

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            if (migrationBuilder == null)
            {
                throw new System.ArgumentNullException(nameof(migrationBuilder));
            }

            migrationBuilder.DropForeignKey(
                name: "FK_Bays_Areas_AreaId",
                table: "Bays");

            migrationBuilder.DropForeignKey(
                name: "FK_Bays_Machines_MachineId",
                table: "Bays");

            migrationBuilder.DropIndex(
                name: "IX_Bays_AreaId",
                table: "Bays");

            migrationBuilder.DropIndex(
                name: "IX_Bays_MachineId",
                table: "Bays");

            migrationBuilder.DropColumn(
                name: "AreaId",
                table: "Bays");

            migrationBuilder.DropColumn(
                name: "MachineId",
                table: "Bays");
        }

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            if (migrationBuilder == null)
            {
                throw new System.ArgumentNullException(nameof(migrationBuilder));
            }

            migrationBuilder.AddColumn<int>(
                name: "AreaId",
                table: "Bays",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "MachineId",
                table: "Bays",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Bays_AreaId",
                table: "Bays",
                column: "AreaId");

            migrationBuilder.CreateIndex(
                name: "IX_Bays_MachineId",
                table: "Bays",
                column: "MachineId");

            migrationBuilder.AddForeignKey(
                name: "FK_Bays_Areas_AreaId",
                table: "Bays",
                column: "AreaId",
                principalTable: "Areas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Bays_Machines_MachineId",
                table: "Bays",
                column: "MachineId",
                principalTable: "Machines",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        #endregion
    }
}
