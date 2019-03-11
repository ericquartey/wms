using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Ferretto.Common.EF.Migrations
{
    public partial class AddDateToMission : Migration
    {
        #region Methods

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            if (migrationBuilder == null)
            {
                throw new System.ArgumentNullException(nameof(migrationBuilder));
            }

            migrationBuilder.DropColumn(
                name: "CreationDate",
                table: "Missions");

            migrationBuilder.DropColumn(
                name: "LastModificationDate",
                table: "Missions");
        }

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            if (migrationBuilder == null)
            {
                throw new System.ArgumentNullException(nameof(migrationBuilder));
            }

            migrationBuilder.AddColumn<DateTime>(
                name: "CreationDate",
                table: "Missions",
                nullable: false,
                defaultValueSql: "GETUTCDATE()");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModificationDate",
                table: "Missions",
                nullable: false,
                defaultValueSql: "GETUTCDATE()");
        }

        #endregion
    }
}
