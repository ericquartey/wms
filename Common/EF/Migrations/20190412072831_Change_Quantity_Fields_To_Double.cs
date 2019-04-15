using Microsoft.EntityFrameworkCore.Migrations;

namespace Ferretto.Common.EF.Migrations
{
    [System.CodeDom.Compiler.GeneratedCode("EntityFramework", "v2.2")]
    public partial class Change_Quantity_Fields_To_Double : Migration
    {
        #region Methods

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "ReservedQuantity",
                table: "SchedulerRequests",
                nullable: true,
                oldClrType: typeof(double),
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "RequestedQuantity",
                table: "SchedulerRequests",
                nullable: true,
                oldClrType: typeof(double),
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "RequestedQuantity",
                table: "Missions",
                nullable: false,
                oldClrType: typeof(double));

            migrationBuilder.AlterColumn<int>(
                name: "DispatchedQuantity",
                table: "Missions",
                nullable: false,
                oldClrType: typeof(double));

            migrationBuilder.AlterColumn<int>(
                name: "MaxCapacity",
                table: "ItemsCompartmentTypes",
                nullable: true,
                oldClrType: typeof(double),
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "RequestedQuantity",
                table: "ItemListRows",
                nullable: false,
                oldClrType: typeof(double));

            migrationBuilder.AlterColumn<int>(
                name: "DispatchedQuantity",
                table: "ItemListRows",
                nullable: false,
                oldClrType: typeof(double));

            migrationBuilder.AlterColumn<int>(
                name: "Stock",
                table: "Compartments",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(double),
                oldDefaultValue: 0.0);

            migrationBuilder.AlterColumn<int>(
                name: "ReservedToStore",
                table: "Compartments",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(double),
                oldDefaultValue: 0.0);

            migrationBuilder.AlterColumn<int>(
                name: "ReservedForPick",
                table: "Compartments",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(double),
                oldDefaultValue: 0.0);
        }

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<double>(
                name: "ReservedQuantity",
                table: "SchedulerRequests",
                nullable: true,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.AlterColumn<double>(
                name: "RequestedQuantity",
                table: "SchedulerRequests",
                nullable: true,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.AlterColumn<double>(
                name: "RequestedQuantity",
                table: "Missions",
                nullable: false,
                oldClrType: typeof(int));

            migrationBuilder.AlterColumn<double>(
                name: "DispatchedQuantity",
                table: "Missions",
                nullable: false,
                oldClrType: typeof(int));

            migrationBuilder.AlterColumn<double>(
                name: "MaxCapacity",
                table: "ItemsCompartmentTypes",
                nullable: true,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.AlterColumn<double>(
                name: "RequestedQuantity",
                table: "ItemListRows",
                nullable: false,
                oldClrType: typeof(int));

            migrationBuilder.AlterColumn<double>(
                name: "DispatchedQuantity",
                table: "ItemListRows",
                nullable: false,
                oldClrType: typeof(int));

            migrationBuilder.AlterColumn<double>(
                name: "Stock",
                table: "Compartments",
                nullable: false,
                defaultValue: 0.0,
                oldClrType: typeof(int),
                oldDefaultValue: 0);

            migrationBuilder.AlterColumn<double>(
                name: "ReservedToStore",
                table: "Compartments",
                nullable: false,
                defaultValue: 0.0,
                oldClrType: typeof(int),
                oldDefaultValue: 0);

            migrationBuilder.AlterColumn<double>(
                name: "ReservedForPick",
                table: "Compartments",
                nullable: false,
                defaultValue: 0.0,
                oldClrType: typeof(int),
                oldDefaultValue: 0);
        }

        #endregion
    }
}
