using Microsoft.EntityFrameworkCore.Migrations;

namespace Ferretto.VW.MAS.DataLayer.Migrations
{
    public partial class WeightingScaleTcp : Migration
    {
        #region Methods

        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE Accessories SET IpAddress = '192.168.0.14', TcpPort = 4001 where Discriminator = 'WeightingScale' ");
        }

        #endregion
    }
}
