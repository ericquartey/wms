using Microsoft.EntityFrameworkCore.Migrations;

namespace Ferretto.Common.EF.Migrations
{
    #pragma warning disable

    public partial class RemoveNullableToMaxCapacityInTableItemCompartmentType : Migration
    {
        #region Methods

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<double>(
                name: "MaxCapacity",
                table: "ItemsCompartmentTypes",
                nullable: true,
                oldClrType: typeof(double));
        }

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<double>(
                name: "MaxCapacity",
                table: "ItemsCompartmentTypes",
                nullable: false,
                oldClrType: typeof(double),
                oldNullable: true);
        }

        #endregion
    }

    #pragma warning restore
}
