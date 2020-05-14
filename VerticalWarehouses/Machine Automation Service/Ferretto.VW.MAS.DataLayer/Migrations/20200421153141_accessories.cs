using Microsoft.EntityFrameworkCore.Migrations;

namespace Ferretto.VW.MAS.DataLayer.Migrations
{
    public partial class accessories : Migration
    {
        #region Methods

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bays_BayAccessories_AccessoriesId",
                table: "Bays");

            migrationBuilder.DropTable(
                name: "BayAccessories");

            migrationBuilder.DropTable(
                name: "Accessories");

            migrationBuilder.DropIndex(
                name: "IX_Lasers_BayId",
                table: "Lasers");

            migrationBuilder.DropIndex(
                name: "IX_Bays_AccessoriesId",
                table: "Bays");

            migrationBuilder.DeleteData(
                table: "ErrorStatistics",
                keyColumn: "Code",
                keyValue: 72);

            migrationBuilder.DeleteData(
                table: "ErrorStatistics",
                keyColumn: "Code",
                keyValue: 73);

            migrationBuilder.DropColumn(
                name: "AccessoriesId",
                table: "Bays");

            migrationBuilder.CreateIndex(
                name: "IX_Lasers_BayId",
                table: "Lasers",
                column: "BayId",
                unique: true);
        }

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            if (migrationBuilder is null)
            {
                throw new System.ArgumentNullException(nameof(migrationBuilder));
            }

            migrationBuilder.AddColumn<int>(
                name: "AccessoriesId",
                table: "Bays",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Accessories",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    IsConfigured = table.Column<string>(),
                    IsEnabled = table.Column<string>(),
                    Discriminator = table.Column<string>(nullable: false),
                    PortName = table.Column<string>(nullable: true),
                    Name = table.Column<string>(nullable: true),
                    IpAddress = table.Column<string>(type: "text", nullable: true),
                    TcpPort = table.Column<int>(nullable: true),
                    Size = table.Column<int>(nullable: true),
                    YOffset = table.Column<double>(nullable: true),
                    ZOffsetLowerPosition = table.Column<double>(nullable: true),
                    ZOffsetUpperPosition = table.Column<double>(nullable: true),
                    TokenReader_PortName = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Accessories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BayAccessories",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AlphaNumericBarId = table.Column<int>(nullable: true),
                    BarcodeReaderId = table.Column<int>(nullable: true),
                    CardReaderId = table.Column<int>(nullable: true),
                    LabelPrinterId = table.Column<int>(nullable: true),
                    LaserPointerId = table.Column<int>(nullable: true),
                    TokenReaderId = table.Column<int>(nullable: true),
                    WeightingScaleId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BayAccessories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BayAccessories_Accessories_AlphaNumericBarId",
                        column: x => x.AlphaNumericBarId,
                        principalTable: "Accessories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BayAccessories_Accessories_BarcodeReaderId",
                        column: x => x.BarcodeReaderId,
                        principalTable: "Accessories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BayAccessories_Accessories_CardReaderId",
                        column: x => x.CardReaderId,
                        principalTable: "Accessories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BayAccessories_Accessories_LabelPrinterId",
                        column: x => x.LabelPrinterId,
                        principalTable: "Accessories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BayAccessories_Accessories_LaserPointerId",
                        column: x => x.LaserPointerId,
                        principalTable: "Accessories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BayAccessories_Accessories_TokenReaderId",
                        column: x => x.TokenReaderId,
                        principalTable: "Accessories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BayAccessories_Accessories_WeightingScaleId",
                        column: x => x.WeightingScaleId,
                        principalTable: "Accessories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Bays_AccessoriesId",
                table: "Bays",
                column: "AccessoriesId");

            migrationBuilder.CreateIndex(
                name: "IX_BayAccessories_AlphaNumericBarId",
                table: "BayAccessories",
                column: "AlphaNumericBarId");

            migrationBuilder.CreateIndex(
                name: "IX_BayAccessories_BarcodeReaderId",
                table: "BayAccessories",
                column: "BarcodeReaderId");

            migrationBuilder.CreateIndex(
                name: "IX_BayAccessories_CardReaderId",
                table: "BayAccessories",
                column: "CardReaderId");

            migrationBuilder.CreateIndex(
                name: "IX_BayAccessories_LabelPrinterId",
                table: "BayAccessories",
                column: "LabelPrinterId");

            migrationBuilder.CreateIndex(
                name: "IX_BayAccessories_LaserPointerId",
                table: "BayAccessories",
                column: "LaserPointerId");

            migrationBuilder.CreateIndex(
                name: "IX_BayAccessories_TokenReaderId",
                table: "BayAccessories",
                column: "TokenReaderId");

            migrationBuilder.CreateIndex(
                name: "IX_BayAccessories_WeightingScaleId",
                table: "BayAccessories",
                column: "WeightingScaleId");
        }

        #endregion
    }
}
