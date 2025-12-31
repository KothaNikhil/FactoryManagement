using Microsoft.EntityFrameworkCore.Migrations;
using System;

#nullable disable

namespace FactoryManagement.Migrations
{
    /// <inheritdoc />
    public partial class AddStockPackagesTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "StockPackages",
                columns: table => new
                {
                    StockPackageId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ItemId = table.Column<int>(type: "INTEGER", nullable: false),
                    PackageSize = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    PackageCount = table.Column<int>(type: "INTEGER", nullable: false),
                    Location = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedByUserId = table.Column<int>(type: "INTEGER", nullable: true),
                    ModifiedByUserId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockPackages", x => x.StockPackageId);
                    table.ForeignKey(
                        name: "FK_StockPackages_Items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Items",
                        principalColumn: "ItemId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StockPackages_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_StockPackages_Users_ModifiedByUserId",
                        column: x => x.ModifiedByUserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StockPackages_CreatedByUserId",
                table: "StockPackages",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_StockPackages_ItemId",
                table: "StockPackages",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_StockPackages_ModifiedByUserId",
                table: "StockPackages",
                column: "ModifiedByUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StockPackages");
        }
    }
}
