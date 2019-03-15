using System;
using System.Linq;
using Ferretto.Common.DataModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Ferretto.Common.EF.Migrations
{
    [System.CodeDom.Compiler.GeneratedCode("EntityFramework", "v2.1")]
    public partial class Add_Timestamping_To_Tables_Items_SchedulerRequests : Migration
    {
        #region Methods

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            if (migrationBuilder == null)
            {
                throw new System.ArgumentNullException(nameof(migrationBuilder));
            }

            migrationBuilder.DropColumn(
                name: "LastModificationDate",
                table: "SchedulerRequests");

            migrationBuilder.DropColumn(
                name: "LastModificationDate",
                table: "LoadingUnits");

            migrationBuilder.DropColumn(
                name: "LastModificationDate",
                table: "Compartments");

            migrationBuilder.AlterColumn<DateTime>(
                name: "LastModificationDate",
                table: "Items",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldDefaultValueSql: "GETUTCDATE()");

            migrationBuilder.AlterColumn<DateTime>(
                name: "LastModificationDate",
                table: "ItemLists",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldDefaultValueSql: "GETUTCDATE()");

            migrationBuilder.AlterColumn<DateTime>(
                name: "LastModificationDate",
                table: "ItemListRows",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldDefaultValueSql: "GETUTCDATE()");
        }

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            if (migrationBuilder == null)
            {
                throw new System.ArgumentNullException(nameof(migrationBuilder));
            }

            var factory = new DatabaseContextFactory();

            using (var db = factory.CreateDbContext(null))
            {
                var now = DateTime.UtcNow;

                migrationBuilder.AddColumn<DateTime>(
                    name: "LastModificationDate",
                    table: "SchedulerRequests",
                    nullable: true,
                    defaultValueSql: "GETUTCDATE()");

                AddDates(migrationBuilder, now, db.SchedulerRequests, nameof(db.SchedulerRequests));

                migrationBuilder.AlterColumn<DateTime>(
                    name: "LastModificationDate",
                    table: "SchedulerRequests",
                    nullable: false,
                    defaultValueSql: "GETUTCDATE()");

                migrationBuilder.AddColumn<DateTime>(
                    name: "LastModificationDate",
                    table: "LoadingUnits",
                    nullable: true,
                    defaultValueSql: "GETUTCDATE()");

                AddDates(migrationBuilder, now, db.LoadingUnits, nameof(db.LoadingUnits));

                migrationBuilder.AlterColumn<DateTime>(
                    name: "LastModificationDate",
                    table: "LoadingUnits",
                    nullable: false,
                    defaultValueSql: "GETUTCDATE()");

                AddDates(migrationBuilder, now, db.Items, nameof(db.Items));
                migrationBuilder.AlterColumn<DateTime>(
                    name: "LastModificationDate",
                    table: "Items",
                    nullable: false,
                    defaultValueSql: "GETUTCDATE()",
                    oldClrType: typeof(DateTime),
                    oldNullable: true);

                AddDates(migrationBuilder, now, db.ItemLists, nameof(db.ItemLists));
                migrationBuilder.AlterColumn<DateTime>(
                    name: "LastModificationDate",
                    table: "ItemLists",
                    nullable: false,
                    defaultValueSql: "GETUTCDATE()",
                    oldClrType: typeof(DateTime),
                    oldNullable: true);

                AddDates(migrationBuilder, now, db.ItemListRows, nameof(db.ItemListRows));
                migrationBuilder.AlterColumn<DateTime>(
                    name: "LastModificationDate",
                    table: "ItemListRows",
                    nullable: false,
                    defaultValueSql: "GETUTCDATE()",
                    oldClrType: typeof(DateTime),
                    oldNullable: true);

                migrationBuilder.AddColumn<DateTime>(
                    name: "LastModificationDate",
                    table: "Compartments",
                    nullable: true,
                    defaultValueSql: "GETUTCDATE()");

                AddDates(migrationBuilder, now, db.Compartments, nameof(db.Compartments));

                migrationBuilder.AlterColumn<DateTime>(
                    name: "LastModificationDate",
                    table: "Compartments",
                    nullable: false,
                    defaultValueSql: "GETUTCDATE()");
            }
        }

        private static void AddDates<T>(MigrationBuilder migrationBuilder, DateTime now, DbSet<T> dbSet, string tableName)
            where T : class, IDataModel
        {
            var ids = dbSet.Select(r => (object)r.Id).ToArray();
#pragma warning disable CA1814 // Multidimensional arrays should not be used

            // Justification: Multidimensional array is required here as an input for migrationBuilder.UpdateData
            var dates = new object[ids.Length, 1];
#pragma warning restore CA1814 // Multidimensional arrays should not be used

            for (var i = 0; i < ids.Length; i++)
            {
                dates[i, 0] = now;
            }

            migrationBuilder.UpdateData(
                table: tableName,
                keyColumn: "Id",
                keyValues: ids,
                columns: new[] { "LastModificationDate" },
                values: dates);
        }

        #endregion
    }
}
