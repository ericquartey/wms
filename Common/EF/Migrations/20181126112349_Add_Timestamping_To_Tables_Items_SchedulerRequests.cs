using System;
using System.Linq;
using Ferretto.Common.DataModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Ferretto.Common.EF.Migrations
{
    public partial class Add_Timestamping_To_Tables_Items_SchedulerRequests : Migration
    {
        #region Methods

        protected override void Down(MigrationBuilder migrationBuilder)
        {
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
            using (var db = new DatabaseContext())
            {
                var now = DateTime.UtcNow;

                migrationBuilder.AddColumn<DateTime>(
                    name: "LastModificationDate",
                    table: "SchedulerRequests",
                    nullable: true,
                    defaultValueSql: "GETUTCDATE()");

                this.AddDates(migrationBuilder, now, db.SchedulerRequests);

                migrationBuilder.AlterColumn<DateTime>(
                    name: "LastModificationDate",
                    table: "SchedulerRequests",
                    nullable: true);

                migrationBuilder.AddColumn<DateTime>(
                    name: "LastModificationDate",
                    table: "LoadingUnits",
                    nullable: true,
                    defaultValueSql: "GETUTCDATE()");

                this.AddDates(migrationBuilder, now, db.LoadingUnits);

                migrationBuilder.AlterColumn<DateTime>(
                    name: "LastModificationDate",
                    table: "LoadingUnits",
                    nullable: false);

                this.AddDates(migrationBuilder, now, db.Items);
                migrationBuilder.AlterColumn<DateTime>(
                    name: "LastModificationDate",
                    table: "Items",
                    nullable: false,
                    defaultValueSql: "GETUTCDATE()",
                    oldClrType: typeof(DateTime),
                    oldNullable: true);

                this.AddDates(migrationBuilder, now, db.ItemLists);
                migrationBuilder.AlterColumn<DateTime>(
                    name: "LastModificationDate",
                    table: "ItemLists",
                    nullable: false,
                    defaultValueSql: "GETUTCDATE()",
                    oldClrType: typeof(DateTime),
                    oldNullable: true);

                this.AddDates(migrationBuilder, now, db.ItemListRows);
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

                this.AddDates(migrationBuilder, now, db.Compartments);

                migrationBuilder.AddColumn<DateTime>(
                    name: "LastModificationDate",
                    table: "Compartments",
                    nullable: false);
            }
        }

        private void AddDates<T>(MigrationBuilder migrationBuilder, DateTime now, DbSet<T> dbSet)
            where T : class, IDataModel
        {
            var ids = dbSet.Select(r => (object)r.Id).ToArray();
            var dates = new object[ids.Length, 1];

            for (var i = 0; i < ids.Length; i++)
            {
                dates[i, 0] = now;
            }

            migrationBuilder.UpdateData(
                table: nameof(DatabaseContext.Compartments),
                keyColumn: "Id",
                keyValues: ids,
                columns: new[] { "LastModificationDate" },
                values: dates
                );
        }

        #endregion Methods
    }
}
