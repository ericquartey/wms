using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Migrations.Operations.Builders;

namespace Ferretto.VW.MAS.DataLayer
{
    public static class MigrationBuilderExtensions
    {
        #region Methods

        private static OperationBuilder<SqlOperation> CreateUser(this MigrationBuilder migrationBuilder)
        {
            return migrationBuilder.Sql(string.Empty);
        }

        #endregion
    }
}
