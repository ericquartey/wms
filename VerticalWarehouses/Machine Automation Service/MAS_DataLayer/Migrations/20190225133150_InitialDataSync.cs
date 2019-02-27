using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Ferretto.VW.MAS_DataLayer.Migrations
{
    public partial class InitialDataSync : Migration
    {
        #region Methods

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                "ConfigurationValues",
                "VarName",
                8L);

            migrationBuilder.DeleteData(
                "ConfigurationValues",
                "VarName",
                9L);
        }

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                "ConfigurationValues",
                new[] {"VarName", "VarType", "VarValue"},
                new object[] {8L, 3L, "169.254.231.248"});

            migrationBuilder.InsertData(
                "ConfigurationValues",
                new[] {"VarName", "VarType", "VarValue"},
                new object[] {9L, 0L, "17221"});
        }

        #endregion
    }
}
